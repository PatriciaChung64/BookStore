using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;

namespace BookStore.Business.Components
{
    public class CatalogueProvider : ICatalogueProvider
    {
        public List<Entities.Book> GetBook(int pOffset, int pCount)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                return (from Book in lContainer.Books.Include(Common.ReflectionUtil.GetPropertyName(() => new Book().Stock))
                       orderby Book.Id
                       select Book).Skip(pOffset).Take(pCount).ToList();
            }
        }


        public Book GetBookById(int pId)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                return (from Book in lContainer.Books.Include(Common.ReflectionUtil.GetPropertyName(() => new Book().Stock))
                        where Book.Id == pId
                        select Book).FirstOrDefault();
            }
        }

        public int getBookStock(int pId)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                int totalStockCount = 0;
                List<Stock> bookStocks = lContainer.Stocks.Where(bStock => bStock.Book.Id == pId).ToList();
                foreach(Stock stock in bookStocks)
                {
                    if (stock.Quantity == null)
                    {
                        totalStockCount += 0;
                    }
                    else
                    {
                        totalStockCount += stock.Quantity.Value;
                    }
                }
                return totalStockCount;
            }
        }
    }
}
