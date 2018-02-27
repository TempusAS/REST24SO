﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace in24seven.Controllers
{
    public class ProjectController : In24SevenController
    {
        /// <summary>
        /// Return a list of Projects 
        /// </summary>
        public List<Models.Project> Get()
        {
            var projectClient = new projectRef.ProjectService() { CookieContainer = GetCookies() };
            var sp = new projectRef.ProjectSearch() { ChangedAfter = new DateTime(2000, 1, 1) };
            var projects = projectClient.GetProjectList(sp);

            var ret = new List<Models.Project>();
            foreach (var project in projects)
                ret.Add(new Models.Project { Id = project.Id, Name = project.Name, CustomerId = project.CustomerId, MultiCustomer = project.MultiCustomer});
            return ret;
        }
    }
}
