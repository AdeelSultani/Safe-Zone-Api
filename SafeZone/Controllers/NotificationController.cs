using SafeZone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;


namespace SafeZone.Controllers
{
    public class NotificationController : ApiController
    {
        SafeZoneEntities db = new SafeZoneEntities();


        [HttpPost]
        //  [Route("api/notification/sendSOSWithFlow")]
       
   
        public async Task<HttpResponseMessage> sendSOS1(int userId, decimal senderlat, decimal senderlng)
        {
            try
            {
                var relations = db.Relation
                    .Where(r => r.userId == userId)
                    .OrderBy(r => r.priority)
                    .ToList();

                if (!relations.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        message = "No family members found",
                        status = "NotFound"
                    });
                }

                var now = DateTime.Now;
                var notification = new Notification
                {
                    userId = userId,
                    senderLatitude = senderlat,
                    senderLongitude = senderlng,
                    notifyDate = now.Date,
                    notifyTime = new TimeSpan(now.Hour, now.Minute, 0),
                    isSeen = false
                };
                db.Notification.Add(notification);
                db.SaveChanges();

                foreach (var rel in relations)
                {
                    notification.recipientId = rel.relatedUser;
                    db.SaveChanges();

                    await Task.Delay(30000);

                    var check = db.Notification.AsNoTracking()
                                  .FirstOrDefault(n => n.id == notification.id);

                    if (check != null && check.isSeen == true)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            message = "SOS accepted by your family member",
                            status = "Accepted"
                        });
                    }
                }

                var existing = db.Notification.FirstOrDefault(n => n.id == notification.id);
                if (existing != null && existing.isSeen == false)
                {
                    db.Notification.Remove(existing);
                    db.SaveChanges();
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    message = "No family member responded",
                    status = "Failed"
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    message = "Something went wrong",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public HttpResponseMessage MarkAsSeen(int notificationId)
        {
            try
            {
                var notification = db.Notification.FirstOrDefault(n => n.id == notificationId);
                if (notification == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Notification not found");
                notification.isSeen = true;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Notification marked as seen");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }


        }


        /// <summary>
        ///https://localhost:44303/api/sos/notifications?userid=1
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>

        [HttpGet]
      //  [Route("api/sos/notifications")]
        public IHttpActionResult GetNotifications(int userId)
        {
            var data = (from n in db.Notification
                        join u in db.UserAccount on n.userId equals u.id
                        where n.recipientId == userId && n.isSeen == false
                        orderby n.id descending
                        select new
                        {
                            n.id,
                            n.userId,
                            n.recipientId,
                            userName = u.name,
                            n.senderLatitude,
                            n.senderLongitude,
                            n.notifyDate,
                            n.notifyTime
                        }).ToList();
            if (data.Count == 0)
                return Ok("No new notifications");

            return Ok(data);
        }
       
[HttpGet]
        public async Task<HttpResponseMessage> SendSos(int id, decimal senderLat, decimal senderLng)
        {
            try
            {
                // ------------------------------------------------------------------
                // 1. Pehle relations lo — simple method syntax
                // ------------------------------------------------------------------
                var relations = db.Relation
                    .Where(r => r.userId == id)
                    .OrderBy(r => r.priority)
                    .ToList();

                if (!relations.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        message = "No family members found",
                        status = "NotFound"
                    });
                }

                // ------------------------------------------------------------------
                // 2. Har relation ke liye alag se location lo aur distance nikalo
                // ------------------------------------------------------------------
                var relationsWithDistance = new List<dynamic>();

                foreach (var r in relations)
                {
                    // Us member ki location lo
                    var loc = db.UserLocation.FirstOrDefault(l => l.userId == r.relatedUser);

                    double distance = double.MaxValue; // default: location nai mili
                    if (loc != null)
                    {
                        distance = CalculateDistanceKm(
                            (double)senderLat, (double)senderLng,
                            (double)loc.latitude, (double)loc.longitude);
                    }

                    relationsWithDistance.Add(new
                    {
                        Relation = r,
                        DistanceKm = distance
                    });
                }

                // ------------------------------------------------------------------
                // 3. Ordering:
                //    Koi bhi < 10km ha → distance se sort (nearest first)
                //    Sab >= 10km hain  → priority se sort
                // ------------------------------------------------------------------
                bool anyNearby = relationsWithDistance.Any(r => r.DistanceKm < 10.0);

                var orderedRelations = anyNearby
                    ? relationsWithDistance.OrderBy(r => r.DistanceKm).ToList()
                    : relationsWithDistance.OrderBy(r => r.Relation.priority).ToList();

                string orderingMethod = anyNearby ? "Distance-Based" : "Priority-Based";

                // ------------------------------------------------------------------
                // 4. Notification ek baar banao
                // ------------------------------------------------------------------
                var now = DateTime.Now;
                var notification = new Notification
                {
                    userId = id,
                    senderLatitude = senderLat,
                    senderLongitude = senderLng,
                    notifyDate = now.Date,
                    notifyTime = new TimeSpan(now.Hour, now.Minute, 0),
                    isSeen = false
                };
                db.Notification.Add(notification);
                db.SaveChanges();

                // ------------------------------------------------------------------
                // 5. Ek ek ko notify karo, 30 sec wait, response check karo
                // ------------------------------------------------------------------
                foreach (var item in orderedRelations)
                {
                    // Recipient update karo
                    var existing = db.Notification.FirstOrDefault(n => n.id == notification.id);
                    if (existing == null) break;

                    existing.recipientId = item.Relation.relatedUser;
                    db.SaveChanges();

                    // 30 second wait
                    await Task.Delay(30000);

                    // Fresh read — us ne dekha ya nahi
                    var check = db.Notification.AsNoTracking()
                                  .FirstOrDefault(n => n.id == notification.id);

                    if (check != null && check.isSeen == true)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            message = "SOS accepted by a family member",
                            status = "Accepted",
                            orderingMethod = orderingMethod,
                            respondedBy = item.Relation.name,
                            relationship = item.Relation.relationship,
                            distanceKm = item.DistanceKm == double.MaxValue
                                                ? "Unknown"
                                                : Math.Round(item.DistanceKm, 2).ToString()
                        });
                    }
                }

                // ------------------------------------------------------------------
                // 6. Kisi ne jawab nai dia — notification delete karo
                // ------------------------------------------------------------------
                var toDelete = db.Notification.FirstOrDefault(n => n.id == notification.id);
                if (toDelete != null)
                {
                    db.Notification.Remove(toDelete);
                    db.SaveChanges();
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    message = "No family member responded",
                    status = "Failed",
                    orderingMethod = orderingMethod
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    message = "An error occurred",
                    error = ex.Message
                });
            }
        }

        // =====================================================================
        // HAVERSINE FORMULA
        // =====================================================================
        private double CalculateDistanceKm(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371.0;
            double dLat = ToRad(lat2 - lat1);
            double dLng = ToRad(lng2 - lng1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                     * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private double ToRad(double deg) => deg * (Math.PI / 180.0);
    }
}
