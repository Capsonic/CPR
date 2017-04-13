using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;

namespace ReportsWEBAPI.Controllers
{
    public class PingController : ApiController
    {
        // GET: api/Ping
        [AllowAnonymous]
        public string Get()
        {

            return "OK";
        }

        [HttpGet Route("api/Ping/auth")]
        public object auth()
        {
            WindowsIdentity user = (WindowsIdentity)User.Identity;

            IEnumerable<NTAccount> groups = from groupIdentity in user.Groups
                                            where groupIdentity.IsValidTargetType(typeof(NTAccount))
                                            select groupIdentity.Translate(typeof(NTAccount)) as NTAccount;

            AuthUser authUser = new AuthUser
            {
                Claims = user.UserClaims.ToList(),
                UserName = user.Name,
                Groups = groups.ToList()
            };

            return authUser;
        }

        private class AuthUser
        {
            public string UserName { get; set; }
            public List<Claim> Claims { get; set; }
            public List<NTAccount> Groups { get; set; }
        }
    }


}
