using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeZone.Models.ownModel
{
    public class SendSos
    {
        public int id { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<int> relatedUser { get; set; }
        public string relationship { get; set; }
        public int priority { get; set; }

        public decimal latitude { get; set; }
        public decimal longitude { get; set; }

    }
}