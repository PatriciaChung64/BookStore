using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;
using Microsoft.Practices.ServiceLocation;
using System.Transactions;

namespace BookStore.Business.Components
{
    public class DeliveryNotificationProvider : IDeliveryNotificationProvider
    {
        public IEmailProvider EmailProvider
        {
            get { return ServiceLocator.Current.GetInstance<IEmailProvider>(); }
        }

        private IOrderProvider OrderProvider
        {
            get { return ServiceLocator.Current.GetInstance<IOrderProvider>();  }
        }

        public void NotifyDeliveryCompletion(Guid pDeliveryId, Entities.DeliveryStatus status)
        {
            //give time for customer to decide whether cancel the order or not
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(15));

            Order lAffectedOrder = RetrieveDeliveryOrder(pDeliveryId);
            Delivery pDel = RetrieveDeliveryByNumber(pDeliveryId);
            if (pDel != null)
            {
                UpdateDeliveryStatus(pDeliveryId, status);
                //if delivery is from bookstore to customer
                if (status == Entities.DeliveryStatus.Delivered && pDel.SourceAddress == "Book Store Address")
                {
                    updateOrderStatus(lAffectedOrder.Id, Entities.OrderStatus.Delivered);
                    EmailProvider.SendMessage(new EmailMessage()
                    {
                        ToAddress = lAffectedOrder.Customer.Email,
                        Message = "Our records show that your order" + lAffectedOrder.OrderNumber +
                        " has been delivered. Thank you for shopping at video store"
                    }) ;
                }

                //otherwise if it is one of the pick-up deliveries, and it must be successfully delivered to enter this loop
                else if (status == Entities.DeliveryStatus.Delivered && lAffectedOrder.OrderStatus == Entities.OrderStatus.Paid)
                {
                    OrderProvider.DepleteStocks(pDel.Id);
                    if (isAllPickedUp(lAffectedOrder.Id))
                    {
                        updateOrderStatus(lAffectedOrder.Id, Entities.OrderStatus.Picked_up);
                        //notify customer the order has been picked up
                        NotifyDeliveryPickUp(pDeliveryId, Entities.DeliveryStatus.PickedUp);

                        //give time for customer to decide whether cancel the order or not
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(15));

                        if (!isDispatched(lAffectedOrder.Id) && GetOrderStatus(lAffectedOrder.Id) != Entities.OrderStatus.Cancelled)
                        {
                            //delivery all the picked-up items in one package from book store to customer
                            OrderProvider.PlaceDeliveryForOrder(lAffectedOrder.Id);

                            //order is now on truck
                            NotifyDeliveryOnTruck(pDeliveryId, Entities.DeliveryStatus.Delivering);
                        }
                    }
                }

                else if (status == Entities.DeliveryStatus.Failed && lAffectedOrder.OrderStatus == Entities.OrderStatus.Paid)
                {
                    //update status to pick up failed
                    updateOrderStatus(lAffectedOrder.Id, Entities.OrderStatus.Pick_Up_Failed);
                    //notify by email
                    EmailProvider.SendMessage(new EmailMessage()
                    {
                        ToAddress = lAffectedOrder.Customer.Email,
                        Message = "Our records show that there was a problem" + lAffectedOrder.OrderNumber +
                        " when picked up your order. Please contact Book Store"
                    });

                }

                else if (status == Entities.DeliveryStatus.Failed && pDel.SourceAddress == "Book Store Address")
                {
                    EmailProvider.SendMessage(new EmailMessage()
                    {
                        ToAddress = lAffectedOrder.Customer.Email,
                        Message = "Our records show that there was a problem" + lAffectedOrder.OrderNumber +
                        " delivering your order. Please contact Book Store"
                    });
                    OrderProvider.CancelOrder(lAffectedOrder.Id);
                }
            }
        }

        //notify the customer that the order has been picked up and in their depot
        public void NotifyDeliveryPickUp(Guid pDeliveryId, DeliveryStatus status)
        {
            Order lAffectedOrder = RetrieveDeliveryOrder(pDeliveryId);
            Delivery pDel = RetrieveDeliveryByNumber(pDeliveryId);
            if (pDel != null)
            {
                if (status == Entities.DeliveryStatus.PickedUp)
                {
                    EmailProvider.SendMessage(new EmailMessage()
                    {
                        ToAddress = lAffectedOrder.Customer.Email,
                        Message = "Our records show that your order" + lAffectedOrder.OrderNumber + " has been picked up and currently in the depot. Thank you for your patient"
                    });
                }
                if (status == Entities.DeliveryStatus.Failed)
                {
                    EmailProvider.SendMessage(new EmailMessage()
                    {
                        ToAddress = lAffectedOrder.Customer.Email,
                        Message = "Our records show that there was a problem" + lAffectedOrder.OrderNumber + " delivering your order. Please contact Book Store"
                    });
                }
            }
        }

        //notify the customer that the order is delievering currenlty
        public void NotifyDeliveryOnTruck(Guid pDeliveryId, DeliveryStatus status)
        {
            Order lAffectedOrder = RetrieveDeliveryOrder(pDeliveryId);
            Delivery pDel = RetrieveDeliveryByNumber(pDeliveryId);
            if (pDel != null)
            {
                if (status == Entities.DeliveryStatus.Delivering)
                {
                    EmailProvider.SendMessage(new EmailMessage()
                    {
                        ToAddress = lAffectedOrder.Customer.Email,
                        Message = "Our records show that your order" + lAffectedOrder.OrderNumber + " is delievering currently. Thank you for your patient"
                    });
                }
                if (status == Entities.DeliveryStatus.Failed)
                {
                    EmailProvider.SendMessage(new EmailMessage()
                    {
                        ToAddress = lAffectedOrder.Customer.Email,
                        Message = "Our records show that there was a problem" + lAffectedOrder.OrderNumber + " delivering your order. Please contact Book Store"
                    });
                }
            }
        }

        private void UpdateDeliveryStatus(Guid pDeliveryId, DeliveryStatus status)
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Delivery pDel = lContainer.Deliveries.Where(del => del.ExternalDeliveryIdentifier == pDeliveryId).FirstOrDefault();
                if (pDel != null)
                {
                    pDel.DeliveryStatus = status;
                    lContainer.SaveChanges();
                    lScope.Complete();
                }
            }
        }

        private Entities.OrderStatus GetOrderStatus(int pOrderId)
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Order pOrder = lContainer.Orders.Where(o => o.Id == pOrderId).FirstOrDefault();
                return pOrder.OrderStatus;
            }
        }

        //check if all pick-up deliveries are successfully delivered
        private bool isAllPickedUp(int pOrderId)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                List<Delivery> pickUpDeliveries = lContainer.Deliveries.Where(pDel => pDel.Order.Id == pOrderId).ToList();
                foreach (Delivery pDel in pickUpDeliveries)
                {
                    if (pDel.DeliveryStatus != Entities.DeliveryStatus.Delivered)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private bool isDispatched(int pOrderId)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                List<Delivery> orderDeliveries = lContainer.Deliveries.Where(pDel => pDel.Order.Id == pOrderId).ToList();
                foreach (Delivery pDel in orderDeliveries)
                {
                    if (pDel.SourceAddress.Equals("Book Store Address"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        //update order status
        private void updateOrderStatus(int pOrderId, Entities.OrderStatus status)
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Order pOrder = lContainer.Orders.Where(o => o.Id == pOrderId).FirstOrDefault();
                pOrder.OrderStatus = status;
                lContainer.SaveChanges();
                lScope.Complete();
            }
        }

        private Delivery RetrieveDeliveryByNumber(Guid pDeliveryId)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                return lContainer.Deliveries.Where(pDel => pDel.ExternalDeliveryIdentifier == pDeliveryId).FirstOrDefault();
            }
        }

        private Order RetrieveDeliveryOrder(Guid pDeliveryId)
        {
 	        using(BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                Delivery lDelivery =  lContainer.Deliveries.Include("Order.Customer").Where((pDel) => pDel.ExternalDeliveryIdentifier == pDeliveryId).FirstOrDefault();
                return lDelivery.Order;
            }
        }
    }


}
