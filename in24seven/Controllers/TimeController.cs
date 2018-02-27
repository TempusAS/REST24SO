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
        private DateTime getdt(string d, string h)
        {
            var dt = new DateTime(int.Parse(d.Substring(0, 4)), int.Parse(d.Substring(5, 2)), int.Parse(d.Substring(8, 2)),
                                  int.Parse(h.Substring(0, 2)), int.Parse(h.Substring(3, 2)),0);
            return dt;
        }
        /// <summary>
        /// Add a new Hour to 24SO using  TimeService.SaveHour 
        /// </summary>
        public void Post(Models.LassReg lr)
        {
            try
            {
                var timeClient = new timeRef.TimeService() { CookieContainer = GetCookies() };
                var projClient = new projectRef.ProjectService() { CookieContainer = GetCookies() };
                var projectId = int.Parse(lr.ProsjektNr);
//                var tasks = projClient.GetProjectTasks(projectId);
                var personClient = new personRef.PersonService() { CookieContainer = GetCookies() };
                var startTime = getdt(lr.Dato, lr.TidLastet);
                var stopTime = getdt(lr.Dato, lr.TidTippet);
                TimeSpan timeSpan = stopTime - startTime;

                var hour24SO = new timeRef.Hour
                {
                    //                    TypeOfWorkId = 1,
                    StartTime = getdt(lr.Dato, lr.TidLastet),
                    StopTime = getdt(lr.Dato, lr.TidTippet),
                    ProjectId = int.Parse(lr.ProsjektNr),
                    Description = lr.kommentarerOmLasset,
                    ProjectTaskId = 0,
                    SalaryTypeId = 0,
                    TotalHours = timeSpan.Hours,
                    ContactId = int.Parse(lr.Sjafor),
                    CustomerId = int.Parse(lr.KundeNr),
                    OrderId = 0,
                    Price = 1,
                    Costs = new timeRef.Cost[0],
                    TotalHoursInvoice = timeSpan.Hours,
                };

                var costTransport = new timeRef.Cost
                {
                    DateRegistered = getdt(lr.Dato, lr.TidLastet),
                    ContactId = hour24SO.ContactId,
                    ProductId = int.Parse(lr.TypeBil),
                    ProjectId = hour24SO.ProjectId,
                    Quantity = 1,
                    Price = 1,
                    Description = "Transport",
                    CustomerId = hour24SO.CustomerId
                };

                var costMasse = new timeRef.Cost
                {
                    DateRegistered = getdt(lr.Dato, lr.TidLastet),
                    ContactId = hour24SO.ContactId,
                    ProductId = int.Parse(lr.Massetype),
                    ProjectId = hour24SO.ProjectId,
                    Quantity = int.Parse(lr.AntallM3),
                    Description = "Masse",
                    CustomerId = hour24SO.CustomerId
                };

//                hour24SO.Costs[0] = costTransport;
//                hour24SO.Costs[1] = costMasse;

                timeClient.SaveHour(hour24SO);
                timeClient.AddCost(costTransport);
                timeClient.AddCost(costMasse);
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw (ex);
            }
        }
    }
}
