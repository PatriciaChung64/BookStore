using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Services.Interfaces;
using BookStore.Business.Components.Interfaces;
using Microsoft.Practices.ServiceLocation;
using BookStore.Services.MessageTypes;

using System.ServiceModel;

namespace BookStore.Services
{
    public class OrderService : IOrderService
    {

        private IOrderProvider OrderProvider
        {
            get
            {
                return ServiceFactory.GetService<IOrderProvider>();
            }
        }

        public void SubmitOrder(Order pOrder)
        {
            try
            {

                //this will break because we haven't written message convertor for the new order
                OrderProvider.SubmitOrder(
                    MessageTypeConverter.Instance.Convert<
                    BookStore.Services.MessageTypes.Order,
                    BookStore.Business.Entities.Order>(pOrder)
                );
            }
            catch (BookStore.Business.Entities.InsufficientStockException ise)
            {
                throw new FaultException<InsufficientStockFault>(
                    new InsufficientStockFault());
            }
        }

        public List<Order> getUserOrders(User pUser)
        {
            List<BookStore.Business.Entities.Order> internalList = OrderProvider.getUserOrder(
                MessageTypeConverter.Instance.Convert<BookStore.Services.MessageTypes.User,
                BookStore.Business.Entities.User>(pUser));

            List<Order> externalList = new List<Order>();

            foreach (var order in internalList)
            {
                /*Order externalEntity = MessageTypeConverter.Instance.
                    Convert<BookStore.Business.Entities.Order,
                    BookStore.Services.MessageTypes.Order>(order);*/
                externalList.Add(getOrderById(order.Id));
            }

            return externalList;
        }

        public Order getOrderById(int orderId)
        {
            return (MessageTypeConverter.Instance.Convert<
                BookStore.Business.Entities.Order, BookStore.Services.MessageTypes.Order>
                (OrderProvider.getOrderById(orderId)));

        }

        public string getOrderNumber(int orderId)
        {
            return OrderProvider.getOrderNumber(orderId);
        }

        //cancel order
        public void CancelOrder(int pOrderId)
        {
            OrderProvider.CancelOrder(pOrderId);
        }
    }
}
