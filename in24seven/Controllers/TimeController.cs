using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

        private int getHash(int fraSted, int tilSted, int lassType, int prosjekt)
        {
            var array = new int[] { fraSted, tilSted, lassType, prosjekt }; 
            int hc = array.Length;
            for (int i = 0; i < array.Length; ++i)
            {
                hc = unchecked(hc * 314159 + array[i]);
            }
            return hc;
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
                if (lr.SkiftStartKl == null)
                    lr.SkiftStartKl = "00:00";
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
                TimeSpan tsProsjektTid = stoppTid - startTid ;
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
                    TypeOfWorkId = typeOfWorkId,
                    NeedApproval = true
                };

                var timerLastetTippet = new timeRef.Hour
                {
                    StartTime = getdt(lr.Dato, lr.TidLastet),
                    StopTime = getdt(lr.Dato, lr.TidTippet),
                    ProjectId = projectId,
                    Description = lr.kommentarerOmLasset +  ( (String.IsNullOrEmpty(lr.foto)) ? "" : "\n" + lr.foto),
                    ProjectTaskId = 0,
                    SalaryTypeId = 0,
                    TotalHours = tsLastetTippet.Hours + (tsLastetTippet.Minutes / 60.0),
                    ContactId = contactId,
                    CustomerId = customerId,
                    OrderId = 0,
                    Price = 1,
                    Costs = new timeRef.Cost[0],
                    TotalHoursInvoice = tsLastetTippet.Hours + (tsLastetTippet.Minutes / 60.0),
                    TypeOfWorkId = typeOfWorkId,
                    NeedApproval = true
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
                    TypeOfWorkId = typeOfWorkId,
                    NeedApproval = true
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

                // Finn prosjekt
                var projectClient = new projectRef.ProjectService() { CookieContainer = GetCookies() };
                var spp = new projectRef.ProjectSearch() { ChangedAfter = new DateTime(2000, 1, 1) };
                var projects = projectClient.GetProjectsDetailed(spp);
                projectRef.Project project = null;
                var ret = new List<Models.Project>();
                foreach (var p in projects)
                    if (p.Id == projectId)
                        project = p;

                        // Finn ventetid
                var spv = new productRef.ProductSearchParameters { CategoryId = 7 };
                var venteProd = productClient.GetProducts(spv, new string[] { "Name", "Id" });
                var ventetidNavn = "";
                var ventetidId = int.Parse(lr.Ventetid);
                foreach (var product in venteProd)
                    if (product.Id == ventetidId)
                        ventetidNavn = product.Name;

                // Finn transporttype
                var spt = new productRef.ProductSearchParameters { CategoryId = 1 };
                var trTyper = productClient.GetProducts(spt, new string[] { "Name", "Id" });
                var transportType = "";
                var transportTypeId = int.Parse(lr.TransportType);
                foreach (var trType in trTyper)
                    if (trType.Id == transportTypeId)
                        transportType = trType.Name;

                // Finn artikkel som prises 
                var lasteSted = lr.LasteSted.Trim();
                if (lasteSted == "På prosjekt")
                    lasteSted = project.Name;
                   
                var artikkel = transportType + " " + lasteSted + " - " + lr.TippetSted;
                var spa = new productRef.ProductSearchParameters { CategoryId = 11 };
                var artikler = productClient.GetProducts(spa, new string[] { "Name", "Id", "CategoryId", "SupplierId" });
                // var firstArtikkel = artikler[0];
                int? artikkelId = null;
                int lastId = 0;
                foreach (var a in artikler)
                    if (a.Name == artikkel)
                        artikkelId = a.Id;
                    else
                        lastId = (lastId < a.Id) ? a.Id : lastId;
                if (artikkelId == null) // finnes ikke opprett
                {
                    var newProd = new productRef.Product() { Name = artikkel, CategoryId = 11};
                    //newProd.CategoryId = firstArtikkel.CategoryId;
                    //newProd.Cost = 0;
                    //newProd.SupplierId = firstArtikkel.SupplierId;
                    var retSave = productClient.SaveProducts(new productRef.Product[] { newProd });
                    artikkelId = retSave[0].Id; // få tak i ny id lastId + 1; // 
                    //var spn = new productRef.ProductSearchParameters { CategoryId = 8 };
                    //var artiklern = productClient.GetProducts(spn, new string[] { "Name", "Id", "CategoryId", "SupplierId" });
                    //foreach (var a in artikler)
                    //    if (a.Name == artikkel)
                    //        artikkelId = a.Id;

                }


                var masseText = "";
                double masse = 0.0;
                if (lr.AntallTonn == "0")
                {
                    masse = double.Parse(lr.AntallM3.Replace(',', '.'), CultureInfo.InvariantCulture);
                    masseText = "M3";
                }
                else
                {
                    masse = double.Parse(lr.AntallTonn.Replace(',', '.'), CultureInfo.InvariantCulture);
                    masseText = "Tonn";
                }

                var antall = 1.0;
                if (transportTypeId == 8 || transportTypeId == 9 || transportTypeId == 10 || transportTypeId == 11)
                    antall = 1;
                else if (transportTypeId == 12 || transportTypeId == 13 || transportTypeId == 14)
                    antall = (lr.ProsjektSluttKl == "00:00") ? 0.0 : tsProsjektTid.Hours + (tsProsjektTid.Minutes / 60.0);
                else if (transportTypeId == 17)
                    antall = double.Parse(lr.AntallM3.Replace(',', '.'), CultureInfo.InvariantCulture);
                else if (transportTypeId == 16)
                    antall = double.Parse(lr.AntallTonn.Replace(',', '.'), CultureInfo.InvariantCulture);

                var costTransport = new timeRef.Cost
                    {
                        DateRegistered = getdt(lr.Dato, lr.TidLastet),
                        ContactId = contactId,
                        ProductId = artikkelId,
                        ProjectId = projectId,
                        Quantity = antall,
                        Price = 1,
                        Description = artikkel, 
                            // bilNr + " " + transportType + " " + lr.LasteSted + " - " + lr.TippetSted,
                        CustomerId = customerId
                    };

                var costVare = new timeRef.Cost
                {
                    DateRegistered = getdt(lr.Dato, lr.TidLastet),
                    ContactId = contactId,
                    ProductId = massetypeId,
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
                    ProductId = ventetidId,
                    ProjectId = projectId,
                    Quantity = dVentetid,
                    Price = 1,
                    Description = bilNr + " " + ventetidNavn,
                    CustomerId = customerId
                };

                var spb = new productRef.ProductSearchParameters { CategoryId = 6 };
                var bomProd = productClient.GetProducts(spb, new string[] { "Name", "Id" });
                if (!string.IsNullOrWhiteSpace(lr.Bomverdier)) {

                    var bomLinjer = lr.Bomverdier.Split(';');
                    foreach(var bomLinje in bomLinjer) {
                        var bomAttr = bomLinje.Split('|');
                        var bomId = int.Parse(bomAttr[0]);

                        int bomPasseringer;
                        if (!int.TryParse(bomAttr[1], out bomPasseringer))
                            bomPasseringer = 1;
                        var bomNavn = "";
                        foreach (var bom in bomProd)
                            if (bom.Id == bomId)
                                bomNavn = bom.Name;
                        if (bomNavn.Length > 0) { 
                            var costBom = new timeRef.Cost
                            {
                                DateRegistered = getdt(lr.Dato, lr.TidLastet),
                                ContactId = contactId,
                                ProductId = bomId,
                                ProjectId = projectId,
                                Quantity = bomPasseringer,
                                Price = 1,
                                Description = bilNr + " bompassering " + bomNavn,
                                CustomerId = customerId
                            };
                            timeClient.AddCost(costBom);
                        }
                    }
                }

                if (lr.SkiftStartKl != "00:00" && timerLastetTippet.TotalHours != 0)
                {
                    Trace.WriteLine("SaveHour timerLastetTippet:" + timerLastetTippet.ToString());
                    timeClient.SaveHour(timerLastetTippet);
                }
                Trace.WriteLine("SaveHour timerStartLastet:" + timerStartLastet.ToString());
                if (timerStartLastet.TotalHours != 0)
                    timeClient.SaveHour(timerStartLastet);
                if (lr.ProsjektSluttKl != "00:00" && timerTippetSlutt.TotalHours != 0)
                {
                    Trace.WriteLine("SaveHour timerTippetSlutt:" + timerTippetSlutt.ToString());
                    timeClient.SaveHour(timerTippetSlutt );
                }
                if (costTransport.Quantity > 0)
                {
                    Trace.WriteLine("AddCost costTransport:" + costTransport.ToString());
                    timeClient.AddCost(costTransport);
                }
                Trace.WriteLine("AddCost costVare:" + costVare.ToString());
                timeClient.AddCost(costVare);
                if (dVentetid > 0.0)
                {
                    Trace.WriteLine("AddCost costVentetid:" + costVentetid.ToString());
                    timeClient.AddCost(costVentetid);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Trace.WriteLine(ex);
                throw (ex);
            }
        }
    }
}
