using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Entities;

namespace BookStore.Business.Components.Interfaces
{
    public interface IDeliveryNotificationProvider
    {

        void NotifyDeliveryCompletion(Guid pDeliveryId, DeliveryStatus status);

        //notify the customer that the order has been picked up and in their depot
        void NotifyDeliveryPickUp(Guid pDeliveryId, DeliveryStatus status);

        //notify the customer that the order is delievering currenlty
        void NotifyDeliveryOnTruck(Guid pDeliveryId, DeliveryStatus status);
    }
}
