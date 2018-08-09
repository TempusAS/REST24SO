using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace in24seven.Controllers
{
    public class ProductController : In24SevenController
    {
        /// <summary>
        /// Return a list of Products. CategoryId must be set
        /// </summary>
        public List<Models.Product> Get(int categoryId)
        {
            try { 
            var productClient = new productRef.ProductService() { CookieContainer = GetCookies() };
            var sp = new productRef.ProductSearchParameters { CategoryId = categoryId};
            var products = productClient.GetProducts(sp, new string[] { "Name", "Id" });
            var ret = new List<Models.Product>();
            foreach (var product in products)
                ret.Add(new Models.Product { Id = product.Id.ToString().Trim(), Name = product.Name + " (" + product.Id.ToString() + ")" });
            return ret.OrderBy(c => c.Name).ToList<Models.Product>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw (ex);
            }

        }
    }
}
