using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;



namespace DF.Emulator
{

    public class Route
    {
        public int RouteId { get; set; }
        public GeoCoordinate RouteStart { get; set; }
        public GeoCoordinate RouteEnd { get; set;  }

        public static double getDistanceInMeters(WayPoint start, WayPoint end)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = deg2rad(end.Latitude - start.Latitude);  // deg2rad below
            var dLon = deg2rad(end.Longitude - end.Latitude);
            var a =
              Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(deg2rad(start.Latitude)) * Math.Cos(deg2rad(end.Latitude)) *
              Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
              ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            d = d * 1000; // Distance in Meters
            return d;
        }

        public static double deg2rad( double deg)
        {
            return deg * (Math.PI / 180);
        }
    }

    
}
