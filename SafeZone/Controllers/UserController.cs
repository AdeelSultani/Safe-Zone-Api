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
    public class UserController : ApiController
    {
        SafeZoneEntities db = new SafeZoneEntities();
        [HttpPost]

        public HttpResponseMessage Signup(UserAccount user)
        {
            String _name = user.name;
            String _password = user.password;
            try
            {

                var data = db.UserAccount.Where(x => x.name == _name && x.password == _password);
                if (data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Change password/password already exist");
                }
                if (user == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                var res = db.UserAccount.Add(user);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, new { res.id, role = "user", res.gender });

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }

        }

        [HttpPost]
        public HttpResponseMessage UserLogin(string name, string password)
        {
            try
            {
                var res = db.UserAccount.FirstOrDefault(x => x.name == name && x.password == password);
                if (res == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User does not exist");
                return Request.CreateResponse(HttpStatusCode.OK, new { res.id, role = "user", res.gender });

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        [HttpGet]
   
        public HttpResponseMessage GetClusteredReports(string gender)
        {
            try
            {
                var reports = db.Report
                    .Where(r => r.affectedgender == gender && r.isVerified == true)
                    .Select(r => new UnApprovedReport
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
                        address = r.address
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

                    var clusterReports = new List<UnApprovedReport>();

                    foreach (var other in reports)
                    {
                        double distance = CalculateDistance(
                            (double)report.latitude,
                            (double)report.longitude,
                            (double)other.latitude,
                            (double)other.longitude
                        );

                        if (distance <= 100)
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

                        string color = hasMurder ? "red": (clusterReports.Count >= 7 ?"red"
        : (clusterReports.Count >= 4 ? "yellow" : null));

                        clusters.Add(new
                        {
                            centerLatitude = centerLat,
                            centerLongitude = centerLng,
                            radius = 50,
                            count = clusterReports.Count,
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

        [HttpGet]
        public HttpResponseMessage filterReports(string category = null, int? time = null)
        {
            List<UnApprovedReport> result = new List<UnApprovedReport>();
            string cat = category?.Trim().ToLower();

            if (string.IsNullOrEmpty(category) || category == "null")
            {
                category = null;
            }
            TimeSpan t7 = new TimeSpan(7, 0, 0);
            TimeSpan t12 = new TimeSpan(12, 0, 0);
            TimeSpan t17 = new TimeSpan(17, 0, 0);
         

            try
            {
                if (category == null && time == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Category and time parameters are required");
                }

                if (category != null && time != null)
                {
                    if (time == 1)
                    {
                        result = db.Report.Where(r => r.reporttime >= t7 &&
                           r.reporttime < t12 &&
                           r.crimetype.ToLower().Equals(cat))
                              .Select(r => new UnApprovedReport
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
                                  address = r.address
                              })
                            .ToList();
                    }
                    else if (time == 2)
                    {
                        result = db.Report.Where(r => r.reporttime >= t12 &&
                           r.reporttime < t17 &&
                           r.crimetype.ToLower().Equals(cat))
                              .Select(r => new UnApprovedReport
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
                                  address = r.address
                              })
                            .ToList();
                    }
                    else if (time == 3)
                    {
                        result = db.Report.Where(r =>
                            (r.reporttime >= t17 || r.reporttime < t7) &&
                            r.crimetype.ToLower().Equals(cat))
                              .Select(r => new UnApprovedReport
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
                                  address = r.address
                              })
                            .ToList();
                    }
                }

                if (time != null && category == null)
                {
                    if (time == 1)
                    {
                        result = db.Report.Where(r => r.reporttime >= t7 &&
                           r.reporttime < t12)
                              .Select(r => new UnApprovedReport
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
                                  address = r.address
                              })
                            .ToList();
                    }
                    else if (time == 2)
                    {
                        result = db.Report.Where(r => r.reporttime >= t12 &&
                           r.reporttime < t17)
                              .Select(r => new UnApprovedReport
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
                                  address = r.address
                              })
                            .ToList();
                    }
                    else if (time == 3)
                    {
                        
                        result = db.Report.Where(r =>r.reporttime >= t17 || r.reporttime < t7)
                              .Select(r => new UnApprovedReport
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
                                  address = r.address
                              })
                            .ToList();
                    }
                }

                if (category != null && time == null)
                {
                    result = db.Report.Where(r => r.crimetype.ToLower().Equals(cat))
                         .Select(r => new UnApprovedReport
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
                             address = r.address
                         })
                        .ToList();
                }

                if (result.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Not Report Found according to your filter");
                }

                return Request.CreateResponse(HttpStatusCode.OK, result);
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
