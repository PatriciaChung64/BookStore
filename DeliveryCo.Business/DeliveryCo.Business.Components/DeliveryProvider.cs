using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeliveryCo.Business.Components.Interfaces;
using System.Transactions;
using DeliveryCo.Business.Entities;
using System.Threading;
using DeliveryCo.Services.Interfaces;


namespace DeliveryCo.Business.Components
{
    public class DeliveryProvider : IDeliveryProvider
    {
        private static Mutex mut = new Mutex();
        public Guid SubmitDelivery(DeliveryCo.Business.Entities.DeliveryInfo pDeliveryInfo)
        {
            using(TransactionScope lScope = new TransactionScope())
            using(DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                pDeliveryInfo.DeliveryIdentifier = Guid.NewGuid();
                pDeliveryInfo.Status = 0;
                lContainer.DeliveryInfo.Add(pDeliveryInfo);
                lContainer.SaveChanges();
                lScope.Complete();
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback((pObj) => ScheduleDelivery(pDeliveryInfo.Id)));
            return pDeliveryInfo.DeliveryIdentifier;
        }


        private void ScheduleDelivery(int pDeliveryInfoId)
        {
            Thread.Sleep(6000);
            //notifying of delivery completion

            mut.WaitOne();
            using (TransactionScope lScope = new TransactionScope())
            using (DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                DeliveryInfo toUpdate = lContainer.DeliveryInfo.Where(i => i.Id == pDeliveryInfoId).FirstOrDefault();
                toUpdate.Status = 1;
                lContainer.SaveChanges();
                lScope.Complete();

                IDeliveryNotificationService lService = DeliveryNotificationServiceFactory.GetDeliveryNotificationService(toUpdate.DeliveryNotificationAddress);
                lService.NotifyDeliveryCompletion(toUpdate.DeliveryIdentifier, DeliveryInfoStatus.Delivered);
            }

            mut.ReleaseMutex();
        }
    }
}
