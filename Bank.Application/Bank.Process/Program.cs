using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Bank.Business.Entities;
using System.ServiceModel;
using Bank.Services;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.ServiceLocatorAdapter;
using Microsoft.Practices.ServiceLocation;
using System.Configuration;

namespace Bank.Process
{
    class Program
    {
        static void Main(string[] args)
        {
            ResolveDependencies();
            CreateDummyEntities();
            HostServices();

        }

        private static void HostServices()
        {
            using (ServiceHost lHost = new ServiceHost(typeof(TransferService)))
            {
                lHost.Open();
                Console.WriteLine("Bank Services started. Press Q to quit.");
                while (Console.ReadKey().Key != ConsoleKey.Q) ;
            }
        }

        private static void CreateDummyEntities()
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BankEntityModelContainer lContainer = new BankEntityModelContainer())
            {
                if (lContainer.Accounts.Count() == 0)
                {
                    Customer lBookStore = new Customer();
                    Account lBSAccount = new Account() { AccountNumber = 123, Balance = 0 };
                    lBookStore.Accounts.Add(lBSAccount);

                    Customer lCustomer = new Customer();
                    Account lCustAccount = new Account() { AccountNumber = 456, Balance = 200 };
                    lCustomer.Accounts.Add(lCustAccount);

                    lContainer.Customers.Add(lBookStore);
                    lContainer.Customers.Add(lCustomer);

                    lContainer.SaveChanges();
                    lScope.Complete();
                }
            }

            // testing Transfer code
            //using (TransactionScope lScope = new TransactionScope())
            //using (BankEntityModelContainer lContainer = new BankEntityModelContainer())
            //{
            //    try
            //    {
            //        int pFromAcctNumber = 456;
            //        int pToAcctNumber = 123;
            //        int pAmount = 50;

            //        // find the two account entities and add them to the Container
            //        Account lFromAcct = lContainer.Accounts.Where(account => pFromAcctNumber == account.AccountNumber).First();
            //        Account lToAcct = lContainer.Accounts.Where(account => pToAcctNumber == account.AccountNumber).First();

            //        // update the two accounts
            //        lFromAcct.Withdraw(pAmount);
            //        lToAcct.Deposit(pAmount);

            //        // save changed entities and finish the transaction
            //        lContainer.SaveChanges();
            //        lScope.Complete();
            //    }
            //    catch (Exception lException)
            //    {
            //        Console.WriteLine("Error occured while transferring money:  " + lException.Message);
            //        throw;
            //    }
            //}
        }

        private static void ResolveDependencies()
        {
            UnityContainer lContainer = new UnityContainer();
            UnityConfigurationSection lSection
                    = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            lSection.Containers["containerOne"].Configure(lContainer);
            UnityServiceLocator locator = new UnityServiceLocator(lContainer);
            ServiceLocator.SetLocatorProvider(() => locator);
        }
    }
}
