using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeZone.Models.ownModel
{
    public class GetFamily
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }
    public class GetFamilyMemberDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string relationship { get; set; }
        public int priority { get; set; }

    }
}