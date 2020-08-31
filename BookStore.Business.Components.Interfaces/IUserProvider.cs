using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Entities;

namespace BookStore.Business.Components.Interfaces
{
    public interface IUserProvider
    {
        void CreateUser(User pUser);

        User ReadUserById(int pUserId);

        void UpdateUser(User pUser);

        void DeleteUser(User pUser);

        bool ValidateUserCredentials(string username, string password);

        Business.Entities.User GetUserByUserNamePassword(string username, string password);

        Business.Entities.User GetUserByUserName(string username);

        Business.Entities.User GetUserByEmail(string email);
    }
}
