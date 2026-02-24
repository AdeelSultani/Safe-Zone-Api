using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeZone.Models.ownModel
{
    public class GeofenceCoordinatesRequest
    {
        public int GeofenceId { get; set; }
        public List<GeofenceCoordinateItem> Coordinates { get; set; }
    }

    public class GeofenceCoordinateItem
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

}