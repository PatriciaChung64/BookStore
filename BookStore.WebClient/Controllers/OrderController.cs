using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStore.Services.MessageTypes;
using BookStore.WebClient.ClientModels;
using BookStore.WebClient.ViewModels;

namespace BookStore.WebClient.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListOrder(UserCache pUser)
        {
            return View(new OrderViewModel(pUser.Model));
        }

        public ActionResult OrderSummary(int pOrderId)
        {
            return View(new OrderViewModel(pOrderId));
        }

        public ActionResult CancelOrder(int pOrderId)
        {
            //placeholder for calling cancel order when merging code
            ServiceFactory.Instance.OrderService.CancelOrder(pOrderId);
            return RedirectToAction("OrderSummary", new { pOrderId = pOrderId });
        }
    }
}