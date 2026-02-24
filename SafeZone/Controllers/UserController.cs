using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SafeZone.Models;

namespace SafeZone.Controllers
{
    public class UserController : ApiController
    {
        SafeZoneEntities db=new SafeZoneEntities();
        [HttpPost]

        public HttpResponseMessage Signup(UserAccount user)
        {
            String _name=user.name;
            String _password=user.password;
            try
            {
               
                var data=db.UserAccount.Where(x=>x.name==_name &&x.password==_password);
                if (data.Any()) {
                    return Request.CreateResponse(HttpStatusCode.Conflict,"Change password/password already exist");
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
                return Request.CreateResponse(HttpStatusCode.OK, new { res.id, role = "user",res.gender });

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

      
    }
}
