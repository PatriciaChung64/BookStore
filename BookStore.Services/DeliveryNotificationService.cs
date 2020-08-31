using DeliveryCo.Services.Interfaces;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;

namespace BookStore.Services
{
    public class DeliveryNotificationService : IDeliveryNotificationService
    {
        IDeliveryNotificationProvider Provider
        {
            get { return ServiceLocator.Current.GetInstance<IDeliveryNotificationProvider>(); }
        }

        public void NotifyDeliveryCompletion(Guid pDeliveryId, DeliveryInfoStatus status)
        {
            Provider.NotifyDeliveryCompletion(pDeliveryId, GetDeliveryStatusFromDeliveryInfoStatus(status));
        }

        //notify the customer that the order has been picked up and in their depot
        public void NotifyDeliveryPickUp(Guid pDeliveryId, DeliveryInfoStatus status)
        {
            Provider.NotifyDeliveryPickUp(pDeliveryId, GetDeliveryStatusFromDeliveryInfoStatus(status));
        }

        //notify the customer that the order is delievering currenlty
        public void NotifyDeliveryOnTruck(Guid pDeliveryId, DeliveryInfoStatus status)
        {
            Provider.NotifyDeliveryOnTruck(pDeliveryId, GetDeliveryStatusFromDeliveryInfoStatus(status));
        }

        private DeliveryStatus GetDeliveryStatusFromDeliveryInfoStatus(DeliveryInfoStatus status)
        {
            if (status == DeliveryInfoStatus.Delivered)
            {
                return DeliveryStatus.Delivered;
            }
            else if (status == DeliveryInfoStatus.Failed)
            {
                return DeliveryStatus.Failed;
            }
            else if (status == DeliveryInfoStatus.Submitted)
            {
                return DeliveryStatus.Submitted;
            }
            else if (status == DeliveryInfoStatus.PickedUp)
            {
                return DeliveryStatus.PickedUp;
            }
            else if (status == DeliveryInfoStatus.Delivering)
            {
                return DeliveryStatus.Delivering;
            }
            else if (status == DeliveryInfoStatus.Cancelled)
            {
                return DeliveryStatus.Cancelled;
            }
            else
            {
                throw new Exception("Unexpected delivery status received");
            }
        }

    }
}
