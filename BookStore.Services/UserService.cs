using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Services.Interfaces;
using BookStore.Business.Components.Interfaces;
using System.ComponentModel.Composition;
using Microsoft.Practices.ServiceLocation;
using BookStore.Services.MessageTypes;


namespace BookStore.Services
{
    public class UserService : IUserService
    {
        private IUserProvider UserProvider
        {
            get
            {
                return ServiceFactory.GetService<IUserProvider>();
            }
        }

        public void CreateUser(User pUser)
        {
            var internalType = MessageTypeConverter.Instance.Convert<
                BookStore.Services.MessageTypes.User, 
                BookStore.Business.Entities.User>(
                pUser);
            UserProvider.CreateUser(internalType);
        }


        public User ReadUserById(int pUserId)
        {
            var externalType = MessageTypeConverter.Instance.Convert<
                BookStore.Business.Entities.User, 
                BookStore.Services.MessageTypes.User>(
                UserProvider.ReadUserById(pUserId));
            return externalType;
        }


        public void UpdateUser(User pUser)
        {
            var internalType = MessageTypeConverter.Instance.Convert<
                BookStore.Services.MessageTypes.User, 
                BookStore.Business.Entities.User>(
                pUser);

            UserProvider.UpdateUser(internalType);
        }

        public void DeleteUser(User pUser)
        {
            var internalType = MessageTypeConverter.Instance.Convert<
                BookStore.Services.MessageTypes.User,
                BookStore.Business.Entities.User>(
                pUser);
            UserProvider.DeleteUser(internalType);
        }


        public bool ValidateUserLoginCredentials(string username, string password)
        {
            return UserProvider.ValidateUserCredentials(username, password);
        }


        public User GetUserByUserNamePassword(string username, string password)
        {
            var externalType = MessageTypeConverter.Instance.Convert<
                BookStore.Business.Entities.User,
                BookStore.Services.MessageTypes.User>(
                UserProvider.GetUserByUserNamePassword(username, password));
            return externalType;
        }

        public User GetUserByUserName(string username)
        {
            var externalType = MessageTypeConverter.Instance.Convert<
                BookStore.Business.Entities.User,
                BookStore.Services.MessageTypes.User>(
                UserProvider.GetUserByUserName(username));
            return externalType;
        }


        public User GetUserByEmail(string email) {
            var externalType = MessageTypeConverter.Instance.Convert<
                BookStore.Business.Entities.User,
                BookStore.Services.MessageTypes.User>(
                UserProvider.GetUserByEmail(email));        

            return externalType;
        }
    }
}
