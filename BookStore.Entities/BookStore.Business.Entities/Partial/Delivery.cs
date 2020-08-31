using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookStore.Business.Entities
{
    //public enum DeliveryStatus { Submitted, Delivered, Failed } -- orginal code

    public enum DeliveryStatus { Submitted, Delivered, Failed, PickedUp, Delivering, Cancelled } /*-- two more status are added: picked up and during delivery*/

    public partial class Delivery
    {
        public DeliveryStatus DeliveryStatus
        {
            get
            {
                return (DeliveryStatus)this.Status;
            }
            set
            {
                this.Status = (int)value;
            }
        }
    }
}
