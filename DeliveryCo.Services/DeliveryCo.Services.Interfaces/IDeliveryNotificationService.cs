using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace DeliveryCo.Services.Interfaces
{
    //public enum DeliveryInfoStatus { Submitted, Delivered, Failed }  -- orginal code
    public enum DeliveryInfoStatus { Submitted, Delivered, Failed, PickedUp, Delivering, Cancelled } /*-- two more status are added: picked up and during delivery*/

    [ServiceContract]
    public interface IDeliveryNotificationService
    {
        [OperationContract]
        void NotifyDeliveryCompletion(Guid pDeliveryId, DeliveryInfoStatus status);

        //notify the customer that the order has been picked up and in their depot
        [OperationContract]
        void NotifyDeliveryPickUp(Guid pDeliveryId, DeliveryInfoStatus status);

        //notify the customer that the order is delievering currenlty
        [OperationContract]
        void NotifyDeliveryOnTruck(Guid pDeliveryId, DeliveryInfoStatus status);
    }
}
