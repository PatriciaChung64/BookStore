using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BookStore.Services.Interfaces;
using BookStore.Services.MessageTypes;

namespace BookStore.WebClient.ViewModels
{
    public class OrderViewModel
    {
        public User user { get; }
        public Order order { get; }
        public OrderViewModel(User pUser)
        {
            user = pUser;
        }

        public OrderViewModel(int pOrderId)
        {
            order = OrderService.getOrderById(pOrderId);
        }

        public IOrderService OrderService
        {
            get
            {
                return ServiceFactory.Instance.OrderService;
            }
        }

        public List<Order> userOrders
        {
            get
            {
                return OrderService.getUserOrders(user);
            }
        }
    }
}