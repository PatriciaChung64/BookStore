using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.WebClient.ViewModels
{
    public class InsufficientStockViewModel
    {
        public InsufficientStockViewModel(String pItemName)
        {
            ItemName = pItemName;
        }

        public String ItemName { get; set; }
    }
}