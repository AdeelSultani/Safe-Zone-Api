using SafeZone.Models;
using SafeZone.Models.ownModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SafeZone.Controllers
{
    public class AdminController : ApiController
    {
        SafeZoneEntities db = new SafeZoneEntities();
        [HttpPost]
        public HttpResponseMessage AdminLogin(string name, string password)
        {
            try
            {
                var res = db.Admin.FirstOrDefault(x => x.name == name && x.password == password);
                if (res == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Admin does not exist");
                return Request.CreateResponse(HttpStatusCode.OK, new { res.id, role = "Admin" });

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage unapproved()
        {
            try
            {
                var res = db.PoliceStation
                    .Where(x => x.isApproved == false)
                    .Select(x => new UnApprovedStationDto
                    {
                        id = x.id,
                        station_name = x.station_name,
                        phone = x.phone,
                        latitude = x.latitude,
                        longitude = x.longitude,
                        isApproved = x.isApproved,
                        address= x.address,
                    })
                    .ToList();

                if (res.Count == 0)
                    return Request.CreateResponse(
                        HttpStatusCode.NotFound,
                        "All Station are Approved"
                    );

                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    e.Message
                );
            }
        }

        [HttpPost]
        public HttpResponseMessage updateStationStatus(int id)
        {
            try
            {
                var station = db.PoliceStation.FirstOrDefault(x => x.id == id);
                if (station == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                if(station.isApproved == true)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict,"Already Approved");
                }
                station.isApproved = true;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Station Approved Successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage addCategory(String name,int Intensity)
        {
            try
            {
                var res = db.CrimeCategory.FirstOrDefault(c => c.crimetype == name);
                if (res != null)
                    return Request.CreateResponse(HttpStatusCode.Conflict);

                CrimeCategory newCategory = new CrimeCategory
                {
                    crimetype = name,
                    Intensity= Intensity,
                };
                db.CrimeCategory.Add(newCategory);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

    }
}
