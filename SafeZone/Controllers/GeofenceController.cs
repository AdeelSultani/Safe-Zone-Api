using SafeZone.Models;
using SafeZone.Models.ownModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SafeZone.Controllers
{
    public class GeofenceController : ApiController
    {
        SafeZoneEntities db = new SafeZoneEntities();

        [HttpPost]
        public HttpResponseMessage saveGeofencename(Geofences geofences)
        {
            int stationid = geofences.policestationid;
            try
            {
                if (geofences == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                var chkstationexist=db.Geofences.FirstOrDefault(x=>x.policestationid== stationid);
                if (chkstationexist != null) {
                    return Request.CreateResponse(HttpStatusCode.Conflict,"Already Exist");
                }
                var data = db.Geofences.Add(geofences);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, data);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPost]
        public HttpResponseMessage saveCoordinates(GeofenceCoordinatesRequest request)
        {
            try
            {
                if (request == null || request.Coordinates == null || request.Coordinates.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No coordinates received");
                }

                for (int i = 0; i < request.Coordinates.Count; i++)
                {
                    var item = request.Coordinates[i];

                    GeofenceCoordinates coord = new GeofenceCoordinates
                    {
                        geofenceid = request.GeofenceId,
                        latitude = item.Latitude,
                        longitude = item.Longitude,
                        OrderIndex = i + 1   // 1 se start
                    };

                   var data= db.GeofenceCoordinates.Add(coord);
                }

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Coordinates Saved Successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        public HttpResponseMessage GetJurisdictions()
        {
            try
            {
                var geofences = db.Geofences
                  .Include("GeofenceCoordinates")
                  .ToList();

                var data = geofences.Select(g => new GetJurisdiction
                {
                    Id = g.id,
                    Name = g.name,
                    PoliceStationId = g.policestationid,

                    Coordinates = g.GeofenceCoordinates
                   .OrderBy(x => x.OrderIndex)
                   .Select(x => new GeofenceCoordinateDTO
                   {
                       Latitude = x.latitude,
                       Longitude = x.longitude,
                       OrderIndex = x.OrderIndex
                   }).ToList()
                }).ToList();


                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

       
    }
}

