using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;

namespace BookStore.Business.Components
{
    public class RoleProvider : IRoleProvider
    {
        public List<Role> GetRolesForUser(Entities.User pUser)
        {
            return GetRolesForUserName(pUser.Name);
        }



        public List<Role> GetRolesForUserName(string pUserName)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                var lUser = lContainer.Users.Include("Roles").FirstOrDefault((pUser) => pUser.LoginCredential.UserName == pUserName);
                return lUser.Roles.ToList();
            }
        }
    }
}
