using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeZone.Models.ownModel
{
    public class UnApprovedStationDto
    {
        public int id { get; set; }
        public string station_name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public bool isApproved { get; set; }
    }

}