using System;
using System.Collections.Generic;

namespace in24seven.Controllers
{
    public class CompanyController : In24SevenController
    {
        public List<Models.Company> Get(string Type = "")
        {
            var companyClient = new companyRef.CompanyService() { CookieContainer = GetCookies()};
            var sp = new companyRef.CompanySearchParameters() { ChangedAfter = new DateTime(2000, 1, 1) };
            var companies = companyClient.GetCompanies(sp, new string[] {"Name", "Type", "Id"});

            var ret = new List<Models.Company>();
            foreach (var company in companies)
                if (company.Type.ToString() == Type || Type.Length == 0 )
                    ret.Add(new Models.Company { Type = company.Type.ToString(), Name = company.Name, Id = company.Id.ToString() });
            return ret;
        }
    }
}
