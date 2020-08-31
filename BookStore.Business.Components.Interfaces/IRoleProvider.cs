using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Entities;

namespace BookStore.Business.Components.Interfaces
{
    public interface IRoleProvider
    {
        List<Role> GetRolesForUser(User pUser);
        List<Role> GetRolesForUserName(String pUserName);
    }
}
