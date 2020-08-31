using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Services.MessageTypes
{
    public class InsufficientStockFault
    {
        public String ItemName { get; set; }
    }
}
