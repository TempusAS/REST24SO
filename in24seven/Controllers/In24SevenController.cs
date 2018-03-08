using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web.Configuration;
using System.Web.Http;

namespace in24seven.Controllers
{
    public class In24SevenController : ApiController
    {
        static private CookieContainer cookieContainer = null;
        static private DateTime? timeOfLastLogin = null;
        
        protected CookieContainer GetCookies()
        {
            return cookieContainer;
        }

        public In24SevenController()
        {
            if (cookieContainer == null || 
                timeOfLastLogin == null || 
                (DateTime.Now.Subtract((DateTime)timeOfLastLogin).Minutes > 2))
            {
                var authClient = new autenticateRef.Authenticate()
                {
                    CookieContainer = new CookieContainer(2)
                };

                var ApplicationIdSetting = ConfigurationManager.AppSettings["ApplicationId"];
                if (String.IsNullOrEmpty(ApplicationIdSetting))
                {
                    FailWith("Missing ApllicationId in web.config");
                }

                var cred = new autenticateRef.Credential
                {
                    ApplicationId = new Guid(ApplicationIdSetting),
                    Username = ConfigurationManager.AppSettings["Username"],
                    Password = GetMD5(ConfigurationManager.AppSettings["Password"]),
                    IdentityId = new Guid("00000000-0000-0000-0000-000000000000")
                };
                var sessionId = authClient.Login(cred);
                if (string.IsNullOrEmpty(sessionId))
                {
                    throw new Exception("Cannot log in");
                }
                cookieContainer = authClient.CookieContainer;
                cookieContainer.Add(new Cookie("ASP.NET_SessionId", sessionId) { Domain = "webservices.24sevenoffice.com" });
                timeOfLastLogin = DateTime.Now;
            }
        }

        void FailWith(string msg)
        {
            throw new HttpResponseException(
                   Request.CreateErrorResponse(HttpStatusCode.NotFound,
                   msg));
        }

        private static string GetMD5(string text)
        {
            byte[] hashValue;
            byte[] message = System.Text.Encoding.Unicode.GetBytes(text);

            MD5 hashString = new MD5CryptoServiceProvider();
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += x.ToString("x2").ToLower();
            }
            return hex;
        }

    }
}
