using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SafeZone.Models;
using SafeZone.Models.ownModel;
namespace SafeZone.Controllers
{
    public class FinalController : ApiController
    {
        SafeZoneEntities db = new SafeZoneEntities();

        [HttpGet]
        public HttpResponseMessage historyByYear(int year)
        {

            try { 
                var data = db.History
                    .Where(r => r.reportdate.Year == year)
                    .Select(r => new ZoneReport
                    {
                        Id = r.Id,
                        stationId = r.stationId,
                        userId = r.userId,
                        crimetype = r.crimetype,
                        reportdate = r.reportdate,
                        reporttime = r.reporttime,
                        description = r.description,
                        latitude = r.latitude,
                        longitude = r.longitude,
                        isVerified = r.isVerified,
                        affectedgender = r.affectedgender,
                        address = r.address,
                        intensity = r.CrimeCategory.Intensity
                    })
                    .ToList();
            
                

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No reports found");
                }

                return Request.CreateResponse(HttpStatusCode.OK, data);
            }

            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage HistoryReportByMonth(int year1, int month)
        {
            try
            {
                var data = db.History
                    .Where(r => r.reportdate.Year == year1 && r.reportdate.Month == month)
                    .Select(r => new ZoneReport
                    {
                        Id = r.Id,
                        stationId = r.stationId,
                        userId = r.userId,
                        crimetype = r.crimetype,
                        reportdate = r.reportdate,
                        reporttime = r.reporttime,
                        description = r.description,
                        latitude = r.latitude,
                        longitude = r.longitude,
                        isVerified = r.isVerified,
                        affectedgender = r.affectedgender,
                        address = r.address,
                        intensity = r.CrimeCategory.Intensity
                    })
                    .ToList();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No reports found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage FamilyMemberCrime(int id)
        {
            try
            {
                var familymembercime=(from r in db.Report join f in db.Relation on 
                                 r.userId equals f.relatedUser where f.userId==id
                                     select new ZoneReport
                                     {
                                         Id = r.Id,
                                         stationId = r.stationId,
                                         userId = r.userId,
                                         crimetype = r.crimetype,
                                         reportdate = r.reportdate,
                                         reporttime = r.reporttime,
                                         description = r.description,
                                         latitude = r.latitude,
                                         longitude = r.longitude,
                                         isVerified = r.isVerified,
                                         affectedgender = r.affectedgender,
                                         address = r.address,
                                         intensity = r.CrimeCategory.Intensity
                                     }).ToList();
                if(familymembercime.Count()==0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No Crime  found in your family member");
                }
                return Request.CreateResponse(HttpStatusCode.OK, familymembercime);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetClusterByYear(int year)
        {
            try
            {
                var reports = db.History
                    .Where(r => r.reportdate.Year == year)
                    .Select(r => new ZoneReport
                    {
                        Id = r.Id,
                        stationId = r.stationId,
                        userId = r.userId,
                        crimetype = r.crimetype,
                        reportdate = r.reportdate,
                        reporttime = r.reporttime,
                        description = r.description,
                        latitude = r.latitude,
                        longitude = r.longitude,
                        isVerified = r.isVerified,
                        affectedgender = r.affectedgender,
                        address = r.address,
                        intensity = r.CrimeCategory.Intensity
                    })
                    .ToList();
                if (reports.Count == 0)
                {
                   return Request.CreateResponse(HttpStatusCode.NotFound, "No reports found");
                }
                var clusters = new List<object>();
                var visited = new HashSet<int>();

                foreach (var report in reports)
                {
                    if (visited.Contains(report.Id))
                        continue;

                    var clusterReports = new List<ZoneReport>();

                    foreach (var other in reports)
                    {
                        double distance = CalculateDistance(
                            (double)report.latitude,
                            (double)report.longitude,
                            (double)other.latitude,
                            (double)other.longitude
                        );

                        if (distance <= 200)
                        {
                            clusterReports.Add(other);
                            visited.Add(other.Id);
                        }
                    }

                    if (clusterReports.Count > 0)
                    {
                        double centerLat = clusterReports.Average(r => (double)r.latitude);
                        double centerLng = clusterReports.Average(r => (double)r.longitude);

                        bool hasMurder = clusterReports.Any(r => r.crimetype.ToLower() == "murder");
                        int IntensitySum = clusterReports.Sum(r => r.intensity);
                        string color = hasMurder ? "red" : (IntensitySum > 15 ? "red"
                          : (IntensitySum > 10 ? "yellow" : null));

                        clusters.Add(new
                        {
                            centerLatitude = centerLat,
                            centerLongitude = centerLng,
                            radius = 90,
                            totalIntensity = IntensitySum,
                            color = color,
                            reports = clusterReports
                        });
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, clusters);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371000;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
