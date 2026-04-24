using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeZone.Models.ownModel
{
    public class UnApprovedReport
    {
        public int Id { get; set; }
        public Nullable<int> stationId { get; set; }
        public Nullable<int> userId { get; set; }
        public string crimetype { get; set; }
        public DateTime reportdate { get; set; }
        public TimeSpan reporttime { get; set; }
        public string description { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
     
        public bool isVerified { get; set; }
        public string affectedgender { get; set; }

        public string address { get; set; }

        //public string stationname { get; set; }
        //public string email { get; set; }
        //public int year { get; set; }
    }
}