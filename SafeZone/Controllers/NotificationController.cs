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
        public async Task<IHttpActionResult> SendSOS(int userId, decimal senderlat, decimal senderlng)
        {
            try
            {
                var relations = db.Relation
                    .Where(r => r.userId == userId)
                    .OrderBy(r => r.priority)
                    .ToList();

                if (!relations.Any())
                    return Ok("No family members found");

                var now = DateTime.Now;

                // 🔹 Create ONE notification row
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
                    // 🔹 UPDATE recipient (NO duplicate insert)
                    notification.recipientId = rel.relatedUser;
                    db.SaveChanges();

                    await Task.Delay(30000);

                    var check = db.Notification.AsNoTracking().FirstOrDefault(n => n.id == notification.id);

                    if (check != null && check.isSeen == true)
                    {
                        return Ok(new
                        {
                            message = "SOS send to your family member",
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

                return Ok(new
                {
                    message = "No family member responded",
                    status = "Failed"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
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
                        join u in db.UserAccount on n.recipientId equals u.id
                        where n.userId == userId && n.isSeen == false
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
    }
}
