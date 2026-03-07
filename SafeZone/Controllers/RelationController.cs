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
    public class RelationController : ApiController
    {
        SafeZoneEntities db = new SafeZoneEntities();

        [HttpPost]
        public HttpResponseMessage MatchContacts([FromBody] List<string> contacts)
        {
            if (contacts == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Contacts list is null");

            var users = db.UserAccount
      .Where(u => contacts.Contains(u.phone))
      .Select(u => new GetFamily
      {
          Id = u.id,
          Name = u.name,
          Phone = u.phone
      })
      .ToList();


            if (users.Count == 0)
                return Request.CreateResponse(HttpStatusCode.NotFound, "No matching contacts found");

            return Request.CreateResponse(HttpStatusCode.OK, users);
        }


        [HttpPost]
        public HttpResponseMessage AddRelation(Relation relation)
        {
            try
            {
               if(relation.userId==relation.relatedUser)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Cannot add relation to self");
                }

             var data=db.Relation.FirstOrDefault(x=>x.userId==relation.userId && x.relatedUser==relation.relatedUser);
                if (data != null){
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Already Added in your Family Member ");
                }
                var priorityData = db.Relation.Where(p => p.priority == relation.priority);
                if (priorityData.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Priority already exists. Please choose a different priority.");
                }
                db.Relation.Add(relation);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Relation added successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
