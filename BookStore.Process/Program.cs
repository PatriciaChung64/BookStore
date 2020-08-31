using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using BookStore.Services;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.ComponentModel.Composition.Hosting;
using BookStore.Services.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity.ServiceLocatorAdapter;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using BookStore.Business.Entities;
using System.Transactions;
using System.ServiceModel.Description;
using BookStore.Business.Components.Interfaces;
using BookStore.WebClient.CustomAuth;

namespace BookStore.Process
{
    public class Program
    {
        static void Main(string[] args)
        {
            ResolveDependencies();
            InsertDummyEntities();
            HostServices();
        }

        private static void InsertDummyEntities()
        {
            InsertCatalogueEntities();
            CreateOperator();
            // #Mark
            CreateUser();
        }

        private static void CreateUser()
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                if (lContainer.Users.Where((pUser) => pUser.Name == "Customer").Count() > 0)
                    return;
            }

            
           // #Mark
            User lCustomer = new User()
            {
                Name = "Customer",
                LoginCredential = new LoginCredential() { UserName = "Customer", Password = "COMP5348" },
                Email = "David@Sydney.edu.au",
                Address = "1 Central Park",
                BankAccountNumber = 456,
            };

            ServiceLocator.Current.GetInstance<IUserProvider>().CreateUser(lCustomer);
            
        }
        

        //private static void InsertCatalogueEntities()
        //{
        //    using (TransactionScope lScope = new TransactionScope())
        //    using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
        //    {
        //        if (lContainer.Books.Count() == 0)
        //        {
        //            // Warehouses.
        //            Warehouse FirstWarehouse = new Warehouse()
        //            {
        //                Address = "John's Fun House"
        //            };

        //            Warehouse SecondWarehouse = new Warehouse()
        //            {
        //                Address = "Keith's Pseudo Schience Workshop"
        //            };

        //            Warehouse ThirdWarehouse = new Warehouse()
        //            {
        //                Address = "Basem's Spaghetti Restaurant "
        //            };

        //            // The book is in stock in multiple warehouses.
        //            Book lGreatExpectations = new Book()
        //            {
        //                Author = "Jane Austen",
        //                Genre = "Fiction",
        //                Price = 20.0,
        //                Title = "Pride and Prejudice"
        //            };

        //            lContainer.Books.Add(lGreatExpectations);

        //            Stock lGreatExpectationsStock = new Stock()
        //            {
        //                Book = lGreatExpectations,
        //                Quantity = 5,
        //                Warehouse = FirstWarehouse
        //            };

        //            lContainer.Stocks.Add(lGreatExpectationsStock);

        //            lGreatExpectationsStock = new Stock()
        //            {
        //                Book = lGreatExpectations,
        //                Quantity = 2,
        //                Warehouse = SecondWarehouse
        //            };

        //            lContainer.Stocks.Add(lGreatExpectationsStock);

        //            lGreatExpectationsStock = new Stock()
        //            {
        //                Book = lGreatExpectations,
        //                Quantity = 4,
        //                Warehouse = ThirdWarehouse
        //            };

        //            lContainer.Stocks.Add(lGreatExpectationsStock);

        //            // The book exists in multiple warehouses but some of them is out of stock.
        //            Book lSoloist = new Book()
        //            {
        //                Author = "Charles Dickens",
        //                Genre = "Fiction",
        //                Price = 15.0,
        //                Title = "Grape Expectations"
        //            };

        //            lContainer.Books.Add(lSoloist);

        //            Stock lSoloistStock = new Stock()
        //            {
        //                Book = lSoloist,
        //                Quantity = 0,
        //                Warehouse = FirstWarehouse
        //            };

        //            lContainer.Stocks.Add(lSoloistStock);

        //            lSoloistStock = new Stock()
        //            {
        //                Book = lSoloist,
        //                Quantity = 7,
        //                Warehouse = SecondWarehouse
        //            };

        //            lContainer.Stocks.Add(lSoloistStock);

        //            lSoloistStock = new Stock()
        //            {
        //                Book = lSoloist,
        //                Quantity = 3,
        //                Warehouse = SecondWarehouse
        //            };

        //            lContainer.Stocks.Add(lSoloistStock);

        //            // The book exists in multiple warehouses but all of them are out of stock.
        //            Book lCookbook = new Book()
        //            {
        //                Author = "Alen Spaghetti",
        //                Genre = "Food",
        //                Price = 15.0,
        //                Title = "The Fine Art of Italian Cooking."
        //            };

        //            lContainer.Books.Add(lCookbook);

        //            Stock lCookbookStock = new Stock()
        //            {
        //                Book = lCookbook,
        //                Quantity = 0,
        //                Warehouse = FirstWarehouse
        //            };

        //            lContainer.Stocks.Add(lCookbookStock);

        //            lCookbookStock = new Stock()
        //            {
        //                Book = lCookbook,
        //                Quantity = 0,
        //                Warehouse = SecondWarehouse
        //            };

        //            lContainer.Stocks.Add(lCookbookStock);

        //            lCookbookStock = new Stock()
        //            {
        //                Book = lCookbook,
        //                Quantity = 0,
        //                Warehouse = SecondWarehouse
        //            };

        //            lContainer.Stocks.Add(lCookbookStock);


        //            // Randomly Allocated Books that only exist in one warehouse.
        //            for (int i = 1; i < 10; i++)
        //            {
        //                Book lItem = new Book()
        //                {
        //                    Author = String.Format("Author {0}", i.ToString()),
        //                    Genre = String.Format("Genre {0}", i),
        //                    Price = i,
        //                    Title = String.Format("Title {0}", i)
        //                };

        //                lContainer.Books.Add(lItem);


        //                Warehouse Selected = FirstWarehouse;
        //                int rotation = i % 3;

        //                if (rotation == 1)
        //                {
        //                    Selected = SecondWarehouse;
        //                }
        //                else if (rotation == 2)
        //                {
        //                    Selected = ThirdWarehouse;
        //                }


        //                Stock lStock = new Stock()
        //                {
        //                    Book = lItem,
        //                    Quantity = 10 + i,
        //                    Warehouse = Selected
        //                };

        //                lContainer.Stocks.Add(lStock);
        //            }

        //            lContainer.SaveChanges();
        //            lScope.Complete();
        //        }
        //    }
        //}

        // add those Icollection in corresponding entities
        private static void InsertCatalogueEntities()
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                if (lContainer.Books.Count() == 0)
                {
                    // Warehouses.
                    Warehouse FirstWarehouse = new Warehouse()
                    {
                        Address = "John's Fun House"
                    };

                    Warehouse SecondWarehouse = new Warehouse()
                    {
                        Address = "Keith's Pseudo Schience Workshop"
                    };

                    Warehouse ThirdWarehouse = new Warehouse()
                    {
                        Address = "Basem's Spaghetti Restaurant "
                    };

                    lContainer.Warehouses.Add(FirstWarehouse);
                    lContainer.Warehouses.Add(SecondWarehouse);
                    lContainer.Warehouses.Add(ThirdWarehouse);

                    // The book is in stock in multiple warehouses.
                    Book lGreatExpectations = new Book()
                    {
                        Author = "Jane Austen",
                        Genre = "Fiction",
                        Price = 20.0,
                        Title = "Pride and Prejudice"
                    };

                    lContainer.Books.Add(lGreatExpectations);

                    Stock lGreatExpectationsStock = new Stock()
                    {
                        Book = lGreatExpectations,
                        Quantity = 5,
                        Warehouse = FirstWarehouse
                    };

                    lContainer.Stocks.Add(lGreatExpectationsStock);

                    Stock lGreatExpectationsStock2 = new Stock()
                    {
                        Id = 1,
                        Book = lGreatExpectations,
                        Quantity = 2,
                        Warehouse = SecondWarehouse
                    };

                    lContainer.Stocks.Add(lGreatExpectationsStock2);

                    lGreatExpectationsStock = new Stock()
                    {
                        Book = lGreatExpectations,
                        Quantity = 4,
                        Warehouse = ThirdWarehouse
                    };

                    lContainer.Stocks.Add(lGreatExpectationsStock);

                    // The book exists in multiple warehouses but some of them is out of stock.
                    Book lSoloist = new Book()
                    {
                        Author = "Charles Dickens",
                        Genre = "Fiction",
                        Price = 15.0,
                        Title = "Grape Expectations"
                    };

                    lContainer.Books.Add(lSoloist);

                    Stock lSoloistStock = new Stock()
                    {
                        Book = lSoloist,
                        Quantity = 0,
                        Warehouse = FirstWarehouse
                    };

                    lContainer.Stocks.Add(lSoloistStock);

                    lSoloistStock = new Stock()
                    {
                        Book = lSoloist,
                        Quantity = 7,
                        Warehouse = SecondWarehouse
                    };

                    lContainer.Stocks.Add(lSoloistStock);

                    lSoloistStock = new Stock()
                    {
                        Book = lSoloist,
                        Quantity = 3,
                        Warehouse = ThirdWarehouse
                    };

                    lContainer.Stocks.Add(lSoloistStock);

                    // The book exists in multiple warehouses but all of them are out of stock.
                    Book lCookbook = new Book()
                    {
                        Author = "Alen Spaghetti",
                        Genre = "Food",
                        Price = 15.0,
                        Title = "The Fine Art of Italian Cooking."
                    };

                    lContainer.Books.Add(lCookbook);

                    Stock lCookbookStock = new Stock()
                    {
                        Book = lCookbook,
                        Quantity = 0,
                        Warehouse = FirstWarehouse
                    };

                    lContainer.Stocks.Add(lCookbookStock);

                    lCookbookStock = new Stock()
                    {
                        Book = lCookbook,
                        Quantity = 0,
                        Warehouse = SecondWarehouse
                    };

                    lContainer.Stocks.Add(lCookbookStock);

                    lCookbookStock = new Stock()
                    {
                        Book = lCookbook,
                        Quantity = 0,
                        Warehouse = ThirdWarehouse
                    };

                    lContainer.Stocks.Add(lCookbookStock);

                    // Randomly Allocated Books that only exist in one warehouse.
                    for (int i = 1; i < 10; i++)
                    {
                        Book lItem = new Book()
                        {
                            Author = String.Format("Author {0}", i.ToString()),
                            Genre = String.Format("Genre {0}", i),
                            Price = i,
                            Title = String.Format("Title {0}", i)
                        };

                        lContainer.Books.Add(lItem);

                        int rotation = i % 3;

                        if (rotation == 0)
                        {
                            Stock lStock = new Stock()
                            {
                                Book = lItem,
                                Quantity = 10 + i,
                                Warehouse = FirstWarehouse
                            };
                            lContainer.Stocks.Add(lStock);
                        }
                        else if (rotation == 1)
                        {
                            Stock lStock = new Stock()
                            {
                                Book = lItem,
                                Quantity = 10 + i,
                                Warehouse = SecondWarehouse
                            };
                            lContainer.Stocks.Add(lStock);
                        }
                        else if (rotation == 2)
                        {
                            Stock lStock = new Stock()
                            {
                                Book = lItem,
                                Quantity = 10 + i,
                                Warehouse = ThirdWarehouse
                            };
                            lContainer.Stocks.Add(lStock);
                        }
                    }

                    lContainer.SaveChanges();
                    lScope.Complete();
                }
            }
        }



        private static void CreateOperator()
        {
            Role lOperatorRole = new Role() { Name = "Operator" };
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                if (lContainer.Roles.Count() > 0)
                {
                    return;
                }
            }
            User lOperator = new User()
            {
                Name = "Operator",
                LoginCredential = new LoginCredential() { UserName = "Operator", Password = "COMP5348" },
                Email = "Wang@Sydney.edu.au",
                Address = "1 Central Park"
            };

            lOperator.Roles.Add(lOperatorRole);

            ServiceLocator.Current.GetInstance<IUserProvider>().CreateUser(lOperator);
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


        private static void HostServices()
        {
            List<ServiceHost> lHosts = new List<ServiceHost>();
            try
            {

                Configuration lAppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ServiceModelSectionGroup lServiceModel = ServiceModelSectionGroup.GetSectionGroup(lAppConfig);

                System.ServiceModel.Configuration.ServicesSection lServices = lServiceModel.Services;
                foreach (ServiceElement lServiceElement in lServices.Services)
                {
                    ServiceHost lHost = new ServiceHost(Type.GetType(GetAssemblyQualifiedServiceName(lServiceElement.Name)));
                    lHost.Open();
                    lHosts.Add(lHost);
                }
                Console.WriteLine("BookStore Service Started, press Q key to quit");
                while (Console.ReadKey().Key != ConsoleKey.Q) ;
            }
            finally
            {
                foreach (ServiceHost lHost in lHosts)
                {
                    lHost.Close();
                }
            }
        }

        private static String GetAssemblyQualifiedServiceName(String pServiceName)
        {
            return String.Format("{0}, {1}", pServiceName, System.Configuration.ConfigurationManager.AppSettings["ServiceAssemblyName"].ToString());
        }
    }
}
