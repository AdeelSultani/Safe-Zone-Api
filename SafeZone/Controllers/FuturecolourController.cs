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
    public class FuturecolourController : ApiController
    {
       SafeZoneEntities db=new SafeZoneEntities();

        [HttpPost]
        public HttpResponseMessage GetFutureClustersWeek(List<ZoneReport> zoneReports, int week)
        {
            try
            {
                if (zoneReports == null || zoneReports.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new List<object>());
                }

                var futureReports = new List<ZoneReport>();
                DateTime now = DateTime.Now;

                // 👉 STEP 1: Week-based filtering
                foreach (var r in zoneReports)
                {
                    double daysPassed = (now - r.reportdate).TotalDays;
                    double futureDays = daysPassed + (week * 7);

                    string crimeType = r.crimetype.Trim().ToLower();

                    bool willExist = true;

                    if (crimeType == "murder")
                    {
                        if (futureDays >= 90)
                            willExist = false;
                    }
                    else
                    {
                        if (futureDays >= 60)
                            willExist = false;
                    }

                    if (willExist)
                    {
                        futureReports.Add(r);
                    }
                }

                if (futureReports.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new List<object>());
                }

                // 👉 STEP 2: Clustering (same as before)
                var clusters = new List<object>();
                var visited = new HashSet<int>();

                foreach (var report in futureReports)
                {
                    if (visited.Contains(report.Id))
                        continue;

                    var clusterReports = new List<ZoneReport>();

                    foreach (var other in futureReports)
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
                        int intensitySum = clusterReports.Sum(r => r.intensity);

                        string color = hasMurder
                            ? "red"
                            : (intensitySum > 15
                                ? "red"
                                : (intensitySum > 10 ? "yellow" : null));

                        clusters.Add(new
                        {
                            centerLatitude = centerLat,
                            centerLongitude = centerLng,
                            radius = 90,
                            totalIntensity = intensitySum,
                            color = color,
                            reports = clusterReports
                        });
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, clusters);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    ex.Message
                );
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
