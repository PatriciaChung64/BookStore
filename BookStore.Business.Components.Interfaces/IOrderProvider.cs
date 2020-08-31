using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Entities;

namespace BookStore.Business.Components.Interfaces
{
    public interface IOrderProvider
    {
        void SubmitOrder(Order pOrder);

        // customer can cancel order
        void CancelOrder(int pOrderId);
        
        void PlaceDeliveryForOrder(int pOrder);

        void DepleteStocks(int pDeliveryId);

        void RefillStockLevels(int pDeliveryId);

        List<Entities.Order> getUserOrder(User pUser);

        Entities.Order getOrderById(int orderId);

        string getOrderNumber(int orderId);
    }
}
