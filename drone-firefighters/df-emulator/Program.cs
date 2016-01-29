using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;

namespace DF.Emulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please insert number of drones and then press enter:");
            string basePath = @"C:\tmp\_ffdrones";
            int numberOfDrones = 1;

            WayPoint building9 = new WayPoint() { Altitude = 0.0, Longitude = -122.122147, Latitude = 47.683388 };
            WayPoint building10 = new WayPoint() { Altitude = 0.0, Longitude = -122.122192, Latitude = 47.684319 };
            WayPoint building2 = new WayPoint() { Altitude = 0.0, Longitude = -122.126129 , Latitude = 47.640682 };
            WayPoint building24 = new WayPoint() { Altitude = 0.0, Longitude = -122.131546, Latitude = 47.641487};
            WayPoint building33 = new WayPoint() { Altitude = 0.0, Longitude = -122.125397, Latitude = 47.642906};

            List<WayPoint> waypoints = new List<WayPoint>();
            waypoints.Add(building9);
            waypoints.Add(building10);
            waypoints.Add(building2);
            waypoints.Add(building24);
            waypoints.Add(building33);

            try
            {
                string numberOfDronesQuery = Console.ReadLine();
                Int32.TryParse(numberOfDronesQuery, out numberOfDrones);

            }
            catch (Exception)
            {

                Console.WriteLine("Not a valid number, using 1 drone");
            }

            // create drone fleet
            List<Drone> PatrolDrones = new List<Drone>();
            List<Drone> FFDrones = new List<Drone>();
            Random randomizer = new Random();
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < numberOfDrones; i++)
            {

                int start = randomizer.Next(0, waypoints.Count - 1);
                int end = randomizer.Next(0, waypoints.Count - 1);
                Drone drone = new Drone("drone" + i.ToString(), DroneType.Patrol, 20, 40);
                drone.Position = waypoints[start];
                drone.Destination = waypoints[end];
                double batteryRandom = randomizer.Next(80, 100);
                drone.BatteryRemaining = batteryRandom/100;
                Route patrolRoute = new Route();
                patrolRoute.RouteStart = new GeoCoordinate() { Latitude = waypoints[start].Latitude, Longitude = waypoints[start].Longitude, Altitude=0};
                patrolRoute.RouteEnd = new GeoCoordinate() { Latitude = waypoints[end].Latitude, Longitude = waypoints[end].Longitude, Altitude = 0 };
                var t = Task.Run(() => drone.Guard(patrolRoute));
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
            //drone1.SaveAsJsonObject(basePath + @"\" + drone1.Droneid + "-" + DateTime.UtcNow.ToString("yyyyMMddThhmmss") + ".json");

        }
    }
}
