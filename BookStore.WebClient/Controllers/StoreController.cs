using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStore.WebClient.ViewModels;

namespace BookStore.WebClient.Controllers
{
    public class StoreController : Controller
    {
        // GET: Store
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListBooks(UserCache pUserCache)
        {
            Console.WriteLine("Logged in as user" + pUserCache.Model.Id);
            return View(new CatalogueViewModel());
        }
    }
}