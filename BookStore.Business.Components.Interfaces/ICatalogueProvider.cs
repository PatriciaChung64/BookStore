using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Entities;

namespace BookStore.Business.Components.Interfaces
{
    public interface ICatalogueProvider
    {
        List<Business.Entities.Book> GetBook(int pOffset, int pCount);
        Book GetBookById(int pId);
        int getBookStock(int pId);
    }
}
