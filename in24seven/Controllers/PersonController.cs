using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace in24seven.Controllers
{
    public class PersonController : In24SevenController
    {
        // Return a list of persons from PersoonService
        public List<Models.Person> Get()
        {
            var personClient = new personRef.PersonService() { CookieContainer = GetCookies()};
            var sp = new personRef.PersonSearchParameters() { ChangedAfter = new DateTime(2000, 1, 1) };
            var persons = personClient.GetPersons(sp);

            var ret = new List<Models.Person>();
            foreach (var person in persons)
                    ret.Add(new Models.Person { Id = person.Id.ToString(), Name = person.FirstName + " " + person.LastName});
            return ret;
        }
    }
}
