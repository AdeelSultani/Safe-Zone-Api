using SafeZone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SafeZone.Controllers
{
    public class ReportController : ApiController
    {

       SafeZoneEntities db=new SafeZoneEntities();

        [HttpGet]
        public HttpResponseMessage getCategory()
        {

            try
            {

                var data = db.CrimeCategory.Select(c=>c.crimetype).ToList();
                if (data.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound); 
                }
                return Request.CreateResponse(HttpStatusCode.OK,data);
            }catch(Exception ex) {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        ////////
        ///
        [HttpPost]
        public HttpResponseMessage SaveReport(Report report)
        {
            try
            {
                if (report == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid report data");
                }

                decimal lat = report.latitude;
                decimal lng = report.longitude;

                var geofences = db.Geofences
                    .Include("GeofenceCoordinates")
                    .ToList();

                foreach (var geofence in geofences)
                {
                    var polygon = geofence.GeofenceCoordinates
                        .OrderBy(c => c.OrderIndex)
                        .Select(c => new
                        {
                            Latitude = c.latitude,
                            Longitude = c.longitude
                        })
                        .ToList<dynamic>();

                    if (IsPointInsidePolygon(lat, lng, polygon))
                    {
                        report.stationId = geofence.policestationid;

                        db.Report.Add(report);
                        db.SaveChanges();

                        var result = new
                        {
                            geofenceName = geofence.name,
                            policeStationId = geofence.policestationid,
                            reportId = report.Id   // return inserted ID
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, result);
                    }
                }

                return Request.CreateResponse( HttpStatusCode.NotFound, "Location not inside any geofence");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // -------------------------
        // Simple Point-in-Polygon function
        private bool IsPointInsidePolygon(decimal lat, decimal lng, List<dynamic> polygon)
        {
            int count = polygon.Count;
            bool inside = false;
            int j = count - 1;

            for (int i = 0; i < count; i++)
            {
                if ((polygon[i].Latitude > lat) != (polygon[j].Latitude > lat) &&
                    (lng < (polygon[j].Longitude - polygon[i].Longitude) *
                    (lat - polygon[i].Latitude) /
                    (polygon[j].Latitude - polygon[i].Latitude) +
                    polygon[i].Longitude))
                {
                    inside = !inside;
                }

                j = i;
            }

            return inside;
        }
    }
}
