using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace in24seven.Controllers
{
    public class VehicleController : In24SevenController
    {
        /// <summary>
        /// Return a list of Vehicles
        /// </summary>
        public List<Models.Vehicle> Get()
        {
            var timeClient = new timeRef.TimeService() { CookieContainer = GetCookies() };
            var wts = new timeRef.WorkTypeSearch() { ProjectId = -1 };
            var workTypeList = timeClient.GetWorkTypeList(wts);
            var ret = new List<Models.Vehicle>();
            foreach (var wt in workTypeList)
                ret.Add(new Models.Vehicle { Id = wt.Id, Name = wt.Name});
            return ret.OrderBy(c => c.Name).ToList<Models.Vehicle>();
        }

    }
}
