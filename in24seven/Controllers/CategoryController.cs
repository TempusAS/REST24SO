using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace in24seven.Controllers
{
    /// <summary>
    /// Get a list of Categories from 24SO ProductService
    /// </summary>
    public class CategoryController : In24SevenController
    {
        /// <summary>
        /// Get a list of Categories from 24SO ProductService
        /// </summary>
        public List<Models.Category> Get()
        {
            var productClient = new productRef.ProductService();
            productClient.CookieContainer = GetCookies();
            var categories = productClient.GetCategories(null);
            var ret = new List<Models.Category>();
            foreach (var category in categories)
                ret.Add(new Models.Category { Nr = category.No, Name = category.Name });

            return ret;

        }
    }
}
