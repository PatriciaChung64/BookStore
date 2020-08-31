using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Components.Interfaces;

namespace BookStore.Business.Components
{
    public class StockProvider : IStockProvider
    {
        public void SellStock(Entities.Stock pStock, int pQuantity)
        {
            throw new NotImplementedException();
        }
    }
}
