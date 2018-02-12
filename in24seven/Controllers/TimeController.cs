using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace in24seven.Controllers
{
    public class TimeController : In24SevenController
    {
        /// <summary>
        /// Add a new Hour to 24SO using  TimeService.SaveHour 
        /// </summary>
        public void Post(Models.Hour hour)
        {
            var timeClient = new timeRef.TimeService() { CookieContainer = GetCookies() };
            var hour24SO = new timeRef.Hour
            {
                StartTime = hour.StartTime,
                ProjectId = hour.ProjectId,
                Description = hour.Description,
                ProjectTaskId = hour.ProjectTaskId,
                SalaryTypeId = hour.SalaryTypeId
            };
            timeClient.SaveHour(hour24SO);
        }
    }
}
