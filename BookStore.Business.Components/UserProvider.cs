using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;
using System.Transactions;
using System.ComponentModel.Composition;

namespace BookStore.Business.Components
{
    public class UserProvider : IUserProvider
    {
        public void CreateUser(User pUser)
        {
            using(TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                lContainer.Users.Add(pUser);
                lContainer.SaveChanges();
                lScope.Complete();
            }
        }


        public User ReadUserById(int pUserId)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                User lCustomer = lContainer.Users.Include("LoginCredential").Where((pUser) => pUser.Id == pUserId).FirstOrDefault();
                return lCustomer;
            }
        }


        public void UpdateUser(User pUser)
        {
            using(TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                lContainer.Users.Add(pUser);
                lContainer.SaveChanges();
                lScope.Complete();
            }
        }


        public void DeleteUser(User pUser)
        {
            using(TransactionScope lScope = new TransactionScope())
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                lContainer.Users.Remove(pUser);
                lContainer.SaveChanges();
            }
        }


        public bool ValidateUserCredentials(string username, string password)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                
                var lCredentials = from lCredential in lContainer.LoginCredentials
                            where lCredential.UserName == username && lCredential.EncryptedPassword == password
                            select lCredential;
                return lCredentials.Count() > 0;
            }
        }


        public User GetUserByUserNamePassword(string username, string password)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                
                var lCredentials = from lCredential in lContainer.LoginCredentials
                            where lCredential.UserName == username && lCredential.EncryptedPassword == password
                            select lCredential;

                if (lCredentials.Count() > 0)
                {
                    LoginCredential lCredential = lCredentials.First();
                    var lUsers = from lCustomer in lContainer.Users
                                 where lCustomer.LoginCredential.Id == lCredential.Id
                                 select lCustomer;
                    if (lUsers.Count() > 0)
                    {
                        return lUsers.First();
                    }
                }
                return null;
            }
        }


        public User GetUserByUserName(string username)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                var lCredentials = from lCredential in lContainer.LoginCredentials
                                   where lCredential.UserName == username
                                   select lCredential;

                if (lCredentials.Count() > 0)
                {
                    LoginCredential lCredential = lCredentials.First();
                    var lUsers = from lCustomer in lContainer.Users
                                 where lCustomer.LoginCredential.Id == lCredential.Id
                                 select lCustomer;
                    if (lUsers.Count() > 0)
                    {

                        var user = lUsers.First();
                        return user;
                    }
                }
                return null;
            }
        }


        public User GetUserByEmail(string email)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                var users = from user in lContainer.Users.Include("LoginCredential")
                            where user.Email == email
                            select user;

                //only allow 1 user per email in system
                if (users.Count() > 0)
                {
                    User user = users.First();
                    return user;
                }
                return null;
            }
        }
    }
}
