using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Device.Location;
using System.Timers;

namespace DF.Emulator
{
    public enum DroneType
    {
        Patrol, FireFighter
    }
    public class WayPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }

    }
    public class Drone
    {
        private static double BatteryLowLimit = 0.10;

        public string Droneid { get; set; }
        public DroneType Type { get; set; }
        public WayPoint Position { get; set; }
        public WayPoint Destination { get; set; }
        public double FlightTime { get; set; }
        public bool GpsStatus { get; set; }
        public double Temp { get; set; }
        public double AirSpeed { get; set; }
        public double BatteryVoltage { get; set; }
        public double BatteryRemaining { get; set; }
        public byte[] ImageCapture { get; set; }

        /// <summary>
        /// Speed in meters per second
        /// </summary>
        public double MaxSpeed { get { return maxSpeedLimit; } }

        double maxSpeedLimit;
        double airborneAltitude;
        private Timer flightTimer;
        private bool Accelerating;
        private bool Decelerating;

        // Drone constructor
        public Drone(string droneId, DroneType droneType, int maxSpeed, double airborneAltitudeSet)
        {
            this.Droneid = droneId;
            this.Type = droneType;
            this.maxSpeedLimit = maxSpeed;
            this.airborneAltitude = airborneAltitudeSet;
            this.flightTimer = new Timer(1000);
            flightTimer.Stop();
            // add event handler for timer tick of flight control
            flightTimer.Elapsed += FlightTimer_Elapsed;
        }

        public void Guard(Route patrolRoute)
        {
            if (!flightTimer.Enabled)
            {
                // drone is not flying
                this.TakeOff();
            }
            while (BatteryRemaining > Drone.BatteryLowLimit)
            {
                GeoCoordinate position = new GeoCoordinate(this.Position.Latitude, this.Position.Longitude);
                double distanceToPatrolEnd = position.GetDistanceTo(patrolRoute.RouteEnd);
                double distanceToPatrolStart = position.GetDistanceTo(patrolRoute.RouteStart);

                if (distanceToPatrolEnd < 20)
                {
                    this.Destination = new WayPoint() {Latitude= patrolRoute.RouteStart.Latitude, Longitude = patrolRoute.RouteStart.Longitude, Altitude=0} ;
                }
                else if (distanceToPatrolStart < 20)
                {
                    this.Destination = new WayPoint() { Latitude = patrolRoute.RouteEnd.Latitude, Longitude = patrolRoute.RouteEnd.Longitude, Altitude = 0 };
                }
                this.Fly();
           }
            // Battery limit is low, go back to base
            this.Destination = new WayPoint() { Latitude = patrolRoute.RouteStart.Latitude, Longitude = patrolRoute.RouteStart.Longitude, Altitude = 0 };
            this.Land();
        }

        public void UpdateFlightData()
        {
            // TODO: implement flight data random generation


        }

        public void Fly()
        {
            // aviation formulas
            if (!flightTimer.Enabled)
            {
                // drone is not flying
                this.TakeOff();
            }
            GeoCoordinate position = new GeoCoordinate(this.Position.Latitude, this.Position.Longitude);
            GeoCoordinate destination = new GeoCoordinate(this.Destination.Latitude, this.Destination.Longitude);
            double distanceToDestination = position.GetDistanceTo(destination);
            while (distanceToDestination > 15 && flightTimer.Enabled) // 1 meter tolerance
            {
                if (this.BatteryRemaining < BatteryLowLimit)
                {
                    Land();
                }

                // accelerate
                this.Accelerating = true;
                if (this.Position.Latitude < this.Destination.Latitude)
                {
                    this.Position.Latitude = this.Position.Latitude + (this.AirSpeed / 100000);
                }
                else
                {
                    this.Position.Latitude = this.Position.Latitude - (this.AirSpeed / 100000);
                }
                if (this.Position.Longitude < this.Destination.Longitude)
                {
                    this.Position.Longitude = this.Position.Longitude + (this.AirSpeed / 100000);
                }
                else
                {
                    this.Position.Longitude = this.Position.Longitude - (this.AirSpeed / 100000);
                }

                position = new GeoCoordinate(this.Position.Latitude, this.Position.Longitude);
                sendTelemetry();
                distanceToDestination = position.GetDistanceTo(destination);
                Console.WriteLine("Drone {0} Distance to destination: {1}", Droneid, distanceToDestination);
                System.Threading.Thread.Sleep(1000);

            }


        }

        public void Land()
        {
            // TODO: implement landing logic

            while (this.Position.Altitude > 0)
            {
                // accelerate
                this.Accelerating = true;
                this.Position.Altitude = this.Position.Altitude - this.AirSpeed;
                System.Threading.Thread.Sleep(1000);

            }
            this.Accelerating = false;
            this.flightTimer.Stop();
        }

        public void TakeOff()
        {
            // TODO: implement takeoff logic
            // start flighttime timer takeoff from base till airborne
            this.flightTimer.Start();
            while (this.Position.Altitude < airborneAltitude)
            {
                // accelerate
                this.Accelerating = true;
                this.Position.Altitude = this.Position.Altitude + this.AirSpeed;
                System.Threading.Thread.Sleep(1000);

            }
            this.Accelerating = false;
        }

        private void FlightTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Speed control
            if (this.Accelerating)
            {
                if (this.AirSpeed < this.maxSpeedLimit - 0.5)
                {
                    this.AirSpeed = this.AirSpeed + 0.5; // accelerating at 0.5 m/s
                }
            }
            else if (this.Decelerating)
            {
                if (this.AirSpeed > 0.5)
                {
                    this.AirSpeed = this.AirSpeed + 0.5; // accelerating at 0.5 m/s
                }
                this.AirSpeed = this.AirSpeed - 0.5; // decelerating at 0.5 m/s
            }
            else
            {
                // keep same speed
            }

            // Telemetry update
            this.updateTelemetry();

            // battery drain
            this.BatteryRemaining = this.BatteryRemaining - 0.0008; // 20 mins duration drains 0.0008 each second
        }


        public void SaveAsJsonObject(string path)
        {
            string serializedDrone = JsonConvert.SerializeObject(this);
            File.WriteAllText(path, serializedDrone);

        }

        private void updateTelemetry()
        {
            Random myrandomizer = new Random();
            this.Temp = myrandomizer.Next(20, 100); // random temp in farheneit
            MessageSender.sendMessage(this);

        }

        private void sendTelemetry()
        {
        }

    }

}
