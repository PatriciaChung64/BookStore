using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Business.Entities
{
    public enum OrderStatus
    {
        Submitted,
        Paid,
        Picked_up,
        Dispatched,
        Delivered,
        Cancelled,
        Pick_Up_Failed
    }
    public partial class Order
    {
        public OrderStatus OrderStatus
        {
            get
            {
                return (OrderStatus)this.Status;
            }
            set
            {
                this.Status = (int)value;
            }
        }

    }
}
