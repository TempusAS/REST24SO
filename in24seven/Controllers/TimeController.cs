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
                var personClient = new personRef.PersonService() { CookieContainer = GetCookies() };
                var startTid = getdt(lr.Dato, lr.SkiftStartKl);
                var lastetTid = getdt(lr.Dato, lr.TidLastet);
                var tippetTid = getdt(lr.Dato, lr.TidTippet);
                if (lr.ProsjektSluttKl == null)
                    lr.ProsjektSluttKl = "00:00";
                if (lr.Merknad == null)
                    lr.Merknad = "";
                if (lr.SkiftStartKl == null)
                    lr.SkiftStartKl = "00:00";
                var stoppTid = getdt(lr.Dato, lr.ProsjektSluttKl);
                TimeSpan tsStartLastet = lastetTid - startTid;
                TimeSpan tsLastetTippet = tippetTid - lastetTid;
                TimeSpan tsTippetSlutt = stoppTid - tippetTid;
                var contactId = int.Parse(lr.Sjafor);
                var customerId = int.Parse(lr.KundeNr);
                var typeOfWorkId = int.Parse(lr.bilNr);

                if (lr.GodkjenteVentetid == null)
                    lr.GodkjenteVentetid = "00:00";
                var aVentetid = lr.GodkjenteVentetid.Split(':');
                double dVentetid = int.Parse(aVentetid[0]) + (int.Parse(aVentetid[1])/60.0);

                var timerStartLastet = new timeRef.Hour
                {
                    StartTime = getdt(lr.Dato, lr.SkiftStartKl),
                    StopTime = getdt(lr.Dato, lr.TidLastet),
                    ProjectId = projectId,
                    Description = lr.kommentarerOmLasset,
                    ProjectTaskId = 0,
                    SalaryTypeId = 0,
                    TotalHours = tsStartLastet.Hours + (tsStartLastet.Minutes / 60.0),
                    ContactId = contactId,
                    CustomerId = customerId,
                    OrderId = 0,
                    Price = 1,
                    Costs = new timeRef.Cost[0],
                    TotalHoursInvoice = tsStartLastet.Hours + (tsStartLastet.Minutes / 60.0),
                    TypeOfWorkId = typeOfWorkId
                   
                };

                var timerLastetTippet = new timeRef.Hour
                {
                    StartTime = getdt(lr.Dato, lr.TidLastet),
                    StopTime = getdt(lr.Dato, lr.TidTippet),
                    ProjectId = projectId,
                    Description = lr.kommentarerOmLasset,
                    ProjectTaskId = 0,
                    SalaryTypeId = 0,
                    TotalHours = tsLastetTippet.Hours + (tsLastetTippet.Minutes / 60.0),
                    ContactId = contactId,
                    CustomerId = customerId,
                    OrderId = 0,
                    Price = 1,
                    Costs = new timeRef.Cost[0],
                    TotalHoursInvoice = tsLastetTippet.Hours + (tsLastetTippet.Minutes / 60.0),
                    TypeOfWorkId = typeOfWorkId
                };

                var timerTippetSlutt = new timeRef.Hour
                {
                    StartTime = getdt(lr.Dato, lr.TidTippet),
                    StopTime = getdt(lr.Dato, lr.ProsjektSluttKl),
                    ProjectId = projectId,
                    Description = lr.kommentarerOmLasset,
                    ProjectTaskId = 0,
                    SalaryTypeId = 0,
                    TotalHours = tsTippetSlutt.Hours + (tsTippetSlutt.Minutes / 60.0),
                    ContactId = contactId,
                    CustomerId = customerId,
                    OrderId = 0,
                    Price = 1,
                    Costs = new timeRef.Cost[0],
                    TotalHoursInvoice = tsTippetSlutt.Hours + (tsTippetSlutt.Minutes / 60.0),
                    TypeOfWorkId = typeOfWorkId
                };

                // Finn bilnummer
                var wts = new timeRef.WorkTypeSearch() { ProjectId = -1 };
                var workTypeList = timeClient.GetWorkTypeList(wts);
                var bilNr = "";
                foreach (var wt in workTypeList)
                    if (wt.Id == typeOfWorkId)
                        bilNr = wt.Name;

                // Finn massetype
                var productClient = new productRef.ProductService() { CookieContainer = GetCookies() };
                var sp = new productRef.ProductSearchParameters { CategoryId = 2 };
                var products = productClient.GetProducts(sp, new string[] { "Name", "Id" });
                var massetype = "";
                var massetypeId = int.Parse(lr.Massetype);
                foreach (var product in products)
                    if (product.Id == massetypeId)
                        massetype = product.Name;

                // Finn ventetid
                var spv = new productRef.ProductSearchParameters { CategoryId = 7 };
                var venteProd = productClient.GetProducts(spv, new string[] { "Name", "Id" });
                var ventetidNavn = "";
                var ventetidId = int.Parse(lr.Ventetid);
                foreach (var product in venteProd)
                    if (product.Id == ventetidId)
                        ventetidNavn = product.Name;

                var masseText = "";
                double masse = 0.0;
                if (lr.AntallM3 != "0")
                {
                    masse = double.Parse(lr.AntallM3);
                    masseText = "M3";
                }
                else
                {
                    masse = double.Parse(lr.AntallTonn);
                    masseText = "Tonn";
                }

                var costTransport = new timeRef.Cost
                {
                    DateRegistered = getdt(lr.Dato, lr.TidLastet),
                    ContactId = contactId,
                    ProductId = int.Parse(lr.TypeBil),
                    ProjectId = projectId,
                    Quantity = masse,
                    Price = 1,
                    Description = bilNr + " Transport " + lr.LasteSted + lr.TippetSted,
                    CustomerId = customerId
                };

                var costVare = new timeRef.Cost
                {
                    DateRegistered = getdt(lr.Dato, lr.TidLastet),
                    ContactId = contactId,
                    ProductId = int.Parse(lr.TypeBil),
                    ProjectId = projectId,
                    Quantity = masse,
                    Price = 1,
                    Description = bilNr + " " + masseText + " " + massetype,
                    CustomerId = customerId
                };

                var costVentetid = new timeRef.Cost
                {
                    DateRegistered = getdt(lr.Dato, lr.TidLastet),
                    ContactId = contactId,
                    ProductId = int.Parse(lr.TypeBil),
                    ProjectId = projectId,
                    Quantity = dVentetid,
                    Price = 1,
                    Description = bilNr + " " + ventetidNavn,
                    CustomerId = customerId
                };

                if (lr.SkiftStartKl != "00:00")
                    timeClient.SaveHour(timerLastetTippet);
                timeClient.SaveHour(timerStartLastet);
                if (lr.ProsjektSluttKl != "00:00")
                    timeClient.SaveHour(timerTippetSlutt);

                timeClient.AddCost(costTransport);
                timeClient.AddCost(costVare);
                if (dVentetid > 0.0)
                    timeClient.AddCost(costVentetid);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw (ex);
            }
        }
    }
}
