using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeZone.Models.ownModel
{
    public class GetJurisdiction
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PoliceStationId { get; set; }
        public List<GeofenceCoordinateDTO> Coordinates { get; set; }
    }
    public class GeofenceCoordinateDTO
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int OrderIndex { get; set; }
    }
}