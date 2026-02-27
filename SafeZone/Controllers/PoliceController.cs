using SafeZone.Models;
using SafeZone.Models.ownModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Web.Http;

namespace SafeZone.Controllers
{
    public class PoliceController : ApiController
    {
        SafeZoneEntities db = new SafeZoneEntities();
        [HttpPost]

        public HttpResponseMessage Signup(PoliceStation policeStation)
        {
            String stationname = policeStation.station_name;
            String _password = policeStation.password;
            try
            {
                if (policeStation == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest);

                var data = db.PoliceStation.Where(x => x.station_name == stationname && x.password == _password);
                if (data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Station already exist");
                }

                var res = db.PoliceStation.Add(policeStation);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, data);

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }

        }
        [HttpPost]

        public HttpResponseMessage Login(String name, String password)
        {
            try
            {
                var data = db.PoliceStation.Where(
                    x => x.station_name == name && password == x.password && x.isApproved == true)
                   .Select(
                        a => new LoginPolice
                        {
                            id = a.id,
                            station_name = a.station_name,
                            address = a.address,
                           
                        }
                        ).FirstOrDefault();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Station not Approved");
                }

                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpPost]

        public HttpResponseMessage unaaprovedReports(int stationid)
        {
            try
            {
                if (stationid == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "ID is null");
                }
                var data = db.Report.Where(x => x.stationId == stationid && x.isVerified == false)
                    .Select(
                     a => new UnApprovedReport
                     {
                        Id = a.Id,
                        userId = a.userId,
                        stationId=a.stationId,
                        crimetype = a.crimetype,
                         isVerified = a.isVerified,
                         reportdate = a.reportdate,
                        reporttime = a.reporttime,
                        description = a.description,
                        latitude = a.latitude,
                        longitude = a.longitude,
                         affectedgender = a.affectedgender,
                         // username = a.UserAccount.name,
                         // stationname = a.PoliceStation.station_name,


                     }
                    )
                    .ToList();

                if(data.Count==0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No report found");
                }

                return Request.CreateResponse(HttpStatusCode.OK,data);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}
