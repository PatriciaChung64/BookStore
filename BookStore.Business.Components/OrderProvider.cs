using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;
using System.Transactions;
using Microsoft.Practices.ServiceLocation;
using DeliveryCo.MessageTypes;
using System.Security.Cryptography;
using System.ServiceModel.Description;

namespace BookStore.Business.Components
{
    public class OrderProvider : IOrderProvider
    {

        public IEmailProvider EmailProvider
        {
            get { return ServiceLocator.Current.GetInstance<IEmailProvider>(); }
        }

        public IUserProvider UserProvider
        {
            get { return ServiceLocator.Current.GetInstance<IUserProvider>(); }
        }

        public void SubmitOrder(Entities.Order pOrder)
        {
            List<Warehouse> warehouses = new List<Warehouse>();
            using (TransactionScope lScope = new TransactionScope())
            {
                //LoadBookStocks(pOrder);
                //MarkAppropriateUnchangedAssociations(pOrder);

                using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
                {
                    try
                    {
                        pOrder.Customer = lContainer.Users.Where(u => u.Id == pOrder.Customer.Id).FirstOrDefault();
                        pOrder.OrderNumber = Guid.NewGuid();
                        pOrder.Store = "OnLine";

                        List<Book> toBuy = new List<Book>();
                        List<int> numToBuy = new List<int>();

                        // Book objects in pOrder are missing the link to their Stock tuple (and the Stock GUID field)
                        // so fix up the 'books' in the order with well-formed 'books' with 1:1 links to Stock tuples
                        foreach (OrderItem lOrderItem in pOrder.OrderItems)
                        {
                            int bookId = lOrderItem.Book.Id;
                            Book toBuyBook = lContainer.Books.Where(book => bookId == book.Id).First();
                            lOrderItem.Book = toBuyBook;
                            toBuy.Add(toBuyBook);
                            numToBuy.Add(lOrderItem.Quantity);

                            //commented out because no longer suitable, Order no longer links to one centralized stock of each book
                            //System.Guid stockId = lOrderItem.Book.Stock.Id;
                            //lOrderItem.Book.Stock = lContainer.Stocks.Where(stock => stockId == stock.Id).First();
                        }


                        //find the optimal number of warehosues to fulfill the order
                        List<KeyValuePair<Warehouse, List<int>>> optimalWarehouse = takeBooksFromWarehouses(toBuy, numToBuy);

                        if (!optimalWarehouse.Any())
                        {
                            throw new InsufficientStockException();
                        }

                        //then, create OrderStock entries based on result
                        ICollection<OrderStock> orderStock = new List<OrderStock>();
                        foreach (var warehousePair in optimalWarehouse)
                        {
                            List<int> currentQuantities = warehousePair.Value;
                            Warehouse currentWarehouse = warehousePair.Key;
                            warehouses.Add(currentWarehouse);
                            for (int i = 0; i < currentQuantities.Count(); i++)
                            {
                                Book currentBook = toBuy[i];
                                if (currentQuantities[i] > 0)
                                {
                                    OrderStock toAdd = new OrderStock
                                    {
                                        Depleted = currentQuantities[i],
                                        Order = pOrder,
                                        Stock = lContainer.Stocks.Where(pStock => pStock.Book.Id == currentBook.Id
                                            && pStock.Warehouse.Id == currentWarehouse.Id).FirstOrDefault()
                                    };
                                    lContainer.OrderStocks.Add(toAdd);
                                    orderStock.Add(toAdd);
                                }
                            }
                        }

                        //after that, reference orderStock in pOrder
                        pOrder.OrderStocks = orderStock;

                        // add the modified Order tree to the Container (in Changed state)
                        lContainer.Orders.Add(pOrder);

                        // ask the Bank service to transfer fundss
                        TransferFundsFromCustomer(UserProvider.ReadUserById(pOrder.Customer.Id).BankAccountNumber, pOrder.Total ?? 0.0);
                        pOrder.OrderStatus = Entities.OrderStatus.Paid;

                        // and save the order
                        lContainer.SaveChanges();
                        lScope.Complete();
                    }
                    catch (Exception lException)
                    {
                        SendOrderErrorMessage(pOrder.Id, lException);
                        IEnumerable<System.Data.Entity.Infrastructure.DbEntityEntry> entries = lContainer.ChangeTracker.Entries();
                        throw;
                    }
                }
            }
            ArrangePickUp(warehouses, pOrder.Id);
            SendOrderPlacedConfirmation(pOrder);
        }

        // customer can cancel order
        public void CancelOrder(int pOrderId)
        {
            //need implementation

            using (TransactionScope lScope = new TransactionScope())
            {

                using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
                {
                    Order pOrder = lContainer.Orders.Include("Customer").Where(o => o.Id == pOrderId).FirstOrDefault();
                    try
                    {
                        if (pOrder != null)
                        { 
                            //order status changed to cancelled
                            pOrder.OrderStatus = Entities.OrderStatus.Cancelled;

                            //change delivery status to fail and refill stock level
                            var list_Delivery = pOrder.Delivery;
                            foreach(Delivery del in list_Delivery)
                            {
                                //only package has not been dispatched can be cancelled.
                                if(del.DestinationAddress == "Book Store Address")
                                {
                                    if(del.DeliveryStatus == DeliveryStatus.Delivered)
                                    {
                                        RefillStockLevels(del.Id);
                                        del.DeliveryStatus = DeliveryStatus.Cancelled;
                                    }
                                }
                            }
                            
                            // ask the Bank service to transfer funds -- refund
                            TransferFundsFromBookStore(UserProvider.ReadUserById(pOrder.Customer.Id).BankAccountNumber, pOrder.Total ?? 0.0);

                            // and save the change
                            lContainer.SaveChanges();
                            lScope.Complete();
                        }
                    }
                    catch (Exception lException)
                    {
                        SendOrderErrorMessage(pOrder.Id, lException);
                        IEnumerable<System.Data.Entity.Infrastructure.DbEntityEntry> entries = lContainer.ChangeTracker.Entries();
                        throw lException;
                    }
                    //notify the customer -- system has issued the refund
                }
            }
            SendRefundConfirmation(pOrderId);
        }

        public void DepleteStocks(int pDeliveryId)
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Delivery pDelivery = lContainer.Deliveries.Where(pDel => pDel.Id == pDeliveryId).FirstOrDefault();
                Order pOrder = pDelivery.Order;

                List<OrderStock> toDeplete = pOrder.OrderStocks.Where(i => i.Stock.Warehouse.Address.Equals(pDelivery.SourceAddress)).ToList();

                foreach(OrderStock orderStock in toDeplete)
                {
                    orderStock.Stock.Quantity -= orderStock.Depleted;
                }

                lContainer.SaveChanges();
                lScope.Complete();
            }
        }

        public List<Entities.Order> getUserOrder(User pUser)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                return (lContainer.Orders
                            .Include("Delivery")
                            .Include("OrderItems")
                            .Include("Customer.LoginCredential")
                            .Where(uOrder => uOrder.Customer.Id == pUser.Id).ToList());
            }
        }

        public Entities.Order getOrderById(int orderId)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Entities.Order found = lContainer.Orders
                            .Include("Delivery")
                            .Include("OrderItems.Book")
                            .Include("Customer.LoginCredential")
                            .Where(order => order.Id == orderId).First();
                

                return (lContainer.Orders
                            .Include("Delivery")
                            .Include("OrderItems.Book")
                            .Include("Customer.LoginCredential")
                            .Where(order => order.Id == orderId).First());
            }
        }

        public string getOrderNumber(int orderId)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                return (lContainer.Orders.Where(order => order.Id == orderId).First().OrderNumber.ToString());
            }
        }


        // Verify if the given combination of warehouses satisfies the requested stocks.
        private bool verify(List<KeyValuePair<Warehouse,List<int>>> combination, List<int> requested)
        {
            // Clone
            List<int> diff = new List<int>(requested);

            // Compute Diff
            for (int i = 0; i < combination.Count; i++)
            {
                for (int j = 0; j < combination[i].Value.Count; j++)
                {
                    diff[j] -= combination[i].Value[j];
                }
            }

            // Verify Diff
            for (int i = 0; i < diff.Count; i++)
            {
                if (diff[i] > 0)
                {
                    return false;
                }
            }

            return true;
        }


        // Given two combinations, select the better combination.
        private List<KeyValuePair<Warehouse, List<int>>> better(List<KeyValuePair<Warehouse, List<int>>> A, List<KeyValuePair<Warehouse, List<int>>> B)
        {
            if (A.Count == 0 && B.Count == 0)
            {
                return A;
            }
            else if (A.Count == 0)
            {
                return B;
            }
            else if (B.Count == 0)
            {
                return A;
            }
            else if (A.Count <= B.Count)
            {
                return A;
            }
            else
            {
                return B;
            }
        }


        // Find the optimal combination of warehouses. - DFS
        private List<KeyValuePair<Warehouse, List<int>>> findWarehouses(List<KeyValuePair<Warehouse, List<int>>> combination, List<KeyValuePair<Warehouse, List<int>>> unused, List<int> requested)
        {
            // A list of VALID combinations.
            List<List<KeyValuePair<Warehouse, List<int>>>> found = new List<List<KeyValuePair<Warehouse, List<int>>>>();

            for (int i = 0; i < unused.Count; i++)
            {
                List<KeyValuePair<Warehouse, List<int>>> nextCombination = new List<KeyValuePair<Warehouse, List<int>>>(combination);
                nextCombination.Add(unused[i]);
                List<KeyValuePair<Warehouse, List<int>>> nextUnused = new List<KeyValuePair<Warehouse, List<int>>>(unused);
                nextUnused.RemoveAt(i);

                found.Add(findWarehouses(nextCombination, nextUnused, requested));
            }

            List<KeyValuePair<Warehouse, List<int>>> best = new List<KeyValuePair<Warehouse, List<int>>>();

            if (verify(combination, requested))
            {
                best = combination;
            }

            for (int i = 0; i < found.Count; i++)
            {
                best = better(best, found[i]);
            }

            return best;
        }

        // Finding out the stock level for each book in each warehouse.
        // Returns [<WAREHOUSE, [STOCKS]>]
        // Note the [STOCKS] is in the order of the given list of books.
        private List<KeyValuePair<Warehouse, List<int>>> getWarehouseStocks(List<Book> books)
        {
            List<KeyValuePair<Warehouse, List<int>>> res = new List<KeyValuePair<Warehouse, List<int>>>();

            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                List<Warehouse> warehouses = lContainer.Warehouses.ToList();

                for (int i = 0; i < warehouses.Count; i++)
                {
                    List<int> stocks = new List<int>();

                    for (int j = 0; j < books.Count; j++)
                    {
                        Book temp_book = books[j];
                        Warehouse temp_warehouse = warehouses[i];
                        List<Stock> bookStockInWarehouse = lContainer.Stocks.Where(x => x.Book.Id == temp_book.Id && x.Warehouse.Id == temp_warehouse.Id).ToList();
                        int stockSum = 0;

                        for (int k = 0; k < bookStockInWarehouse.Count; k++)
                        {
                            if(bookStockInWarehouse[k].Quantity.HasValue)
                            {
                                stockSum += bookStockInWarehouse[k].Quantity.Value;
                            }
                        }

                        stocks.Add(stockSum);
                    }

                    KeyValuePair<Warehouse, List<int>> warehouseStock = new KeyValuePair<Warehouse, List<int>>(warehouses[i], stocks);
                    res.Add(warehouseStock);
                }
            }

            return res;
        }

        private List<KeyValuePair<Warehouse, List<int>>> warehosueStocksTaken(List<KeyValuePair<Warehouse, List<int>>> warehouses, List<int> requested)
        {
            List<KeyValuePair<Warehouse, List<int>>> res = new List<KeyValuePair<Warehouse, List<int>>>();

            for (int i = 0; i < warehouses.Count; i++)
            {
                List<int> taken = new List<int>();

                for (int j = 0; j < warehouses[i].Value.Count; j++)
                {
                    if (warehouses[i].Value[j] >= requested[j])
                    {
                        taken.Add(requested[j]);
                        requested[j] = 0;
                    }
                    else
                    {
                        taken.Add(warehouses[i].Value[j]);
                        requested[j] -= warehouses[i].Value[j];
                    }
                }
                res.Add(new KeyValuePair<Warehouse, List<int>>(warehouses[i].Key, taken));
            }

            return res;
        }

        private List<KeyValuePair<Warehouse, List<int>>> takeBooksFromWarehouses(List<Book> books, List<int> quantities)
        {
            List<KeyValuePair<Warehouse, List<int>>> warehouseStocks = getWarehouseStocks(books);
            List<KeyValuePair<Warehouse, List<int>>> warehousesCombination = findWarehouses(new List<KeyValuePair<Warehouse, List<int>>>(), warehouseStocks, quantities);
            List<KeyValuePair<Warehouse, List<int>>> stocksTaken = warehosueStocksTaken(warehousesCombination, quantities);

            return stocksTaken;
        }

        // restore stock level correspondingly when customers cancelled order
        public void RefillStockLevels(int pDeliveryId)
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Delivery pDelivery = lContainer.Deliveries.Where(pDel => pDel.Id == pDeliveryId).FirstOrDefault();
                Order pOrder = pDelivery.Order;

                List<OrderStock> toDeplete = pOrder.OrderStocks.Where(i => i.Stock.Warehouse.Address.Equals(pDelivery.SourceAddress)).ToList();

                foreach (OrderStock orderStock in toDeplete)
                {
                    orderStock.Stock.Quantity += orderStock.Depleted;
                }

                lContainer.SaveChanges();
                lScope.Complete();
            }
        }

        private void SendOrderErrorMessage(int pOrderId, Exception pException)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Order pOrder = lContainer.Orders.Where(o => o.Id == pOrderId).First();
                EmailProvider.SendMessage(new EmailMessage()
                {
                    ToAddress = pOrder.Customer.Email,
                    Message = "There was an error in processsing your order " + pOrder.OrderNumber + ": " + pException.Message + ". Please contact Book Store"
                });
            }
        }

        private void SendOrderPlacedConfirmation(Order pOrder)
        {
            EmailProvider.SendMessage(new EmailMessage()
            {
                ToAddress = pOrder.Customer.Email,
                Message = "Your order " + pOrder.OrderNumber + " has been placed"
            });
        }

        //refund notification
        private void SendRefundConfirmation(int pOrderId)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Order pOrder = lContainer.Orders.Where(o => o.Id == pOrderId).First();
                EmailProvider.SendMessage(new EmailMessage()
                {
                    ToAddress = pOrder.Customer.Email,
                    Message = "Your order " + pOrder.OrderNumber + " has been cancelled and refunded."
                });
            }
        }


        private void ArrangePickUp(List<Warehouse> warehouses, int pOrderId)
        {
            using (TransactionScope lScope = new TransactionScope())
            {
                using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
                {
                    ICollection<Delivery> pickUpDeliveries = new List<Delivery>();
                    Order pOrder = lContainer.Orders.Where(o => o.Id == pOrderId).FirstOrDefault();
                    foreach (Warehouse toDeliver in warehouses)
                    {
                        Delivery lDelivery = new Delivery()
                        {
                            DeliveryStatus = DeliveryStatus.Submitted,
                            SourceAddress = toDeliver.Address,
                            DestinationAddress = "Book Store Address",
                            Order = pOrder
                        };

                        Guid lDeliveryIdentifier = ExternalServiceFactory.Instance.DeliveryService.SubmitDelivery(new DeliveryInfo()
                        {
                            OrderNumber = lDelivery.Order.OrderNumber.ToString(),
                            SourceAddress = lDelivery.SourceAddress,
                            DestinationAddress = lDelivery.DestinationAddress,
                            DeliveryNotificationAddress = "net.tcp://localhost:9010/DeliveryNotificationService"
                        });

                        lDelivery.ExternalDeliveryIdentifier = lDeliveryIdentifier;

                        lContainer.Deliveries.Add(lDelivery);
                        pickUpDeliveries.Add(lDelivery);
                    }

                    pOrder.Delivery = pickUpDeliveries;

                    lContainer.SaveChanges();
                    lScope.Complete();
                }
            }
        }

        public void PlaceDeliveryForOrder(int pOrderId)
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Order pOrder = lContainer.Orders.Where(o => o.Id == pOrderId).FirstOrDefault();
                Delivery lDelivery = new Delivery()
                {
                    DeliveryStatus = DeliveryStatus.Submitted,
                    SourceAddress = "Book Store Address",
                    DestinationAddress = pOrder.Customer.Address,
                    Order = pOrder
                };

                Guid lDeliveryIdentifier = ExternalServiceFactory.Instance.DeliveryService.SubmitDelivery(new DeliveryInfo()
                {
                    OrderNumber = lDelivery.Order.OrderNumber.ToString(),
                    SourceAddress = lDelivery.SourceAddress,
                    DestinationAddress = lDelivery.DestinationAddress,
                    DeliveryNotificationAddress = "net.tcp://localhost:9010/DeliveryNotificationService"
                });

                lDelivery.ExternalDeliveryIdentifier = lDeliveryIdentifier;
                pOrder.Delivery.Add(lDelivery);
                pOrder.OrderStatus = Entities.OrderStatus.Dispatched;

                lContainer.Deliveries.Add(lDelivery);

                lContainer.SaveChanges();
                lScope.Complete();

            }
        }

        private void TransferFundsFromCustomer(int pCustomerAccountNumber, double pTotal)
        {
            try
            {
                ExternalServiceFactory.Instance.TransferService.Transfer(pTotal, pCustomerAccountNumber, RetrieveBookStoreAccountNumber());
            }
            catch
            {
                throw new Exception("Error when transferring funds for order.");
            }
        }

        //customer get refund
        private void TransferFundsFromBookStore(int pCustomerAccountNumber, double pTotal)
        {
            try
            {
                ExternalServiceFactory.Instance.TransferService.Transfer(pTotal, RetrieveBookStoreAccountNumber(), pCustomerAccountNumber);
            }
            catch
            {
                throw new Exception("Error when transferring funds for order.");
            }
        }


        private int RetrieveBookStoreAccountNumber()
        {
            return 123;
        }

    }
}
