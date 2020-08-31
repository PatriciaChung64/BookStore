using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Services.Interfaces;
using BookStore.Business.Components.Interfaces;
using Microsoft.Practices.ServiceLocation;
using BookStore.Services.MessageTypes;

namespace BookStore.Services
{
    public class RoleService : IRoleService
    {

        private IRoleProvider RoleProvider
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IRoleProvider>();
            }
        }

        public List<Role> GetRolesForUser(User pUser)
        {
            var internalType = MessageTypeConverter.Instance.Convert<
                BookStore.Services.MessageTypes.User,
                BookStore.Business.Entities.User>(
                pUser);

            var internalResult = RoleProvider.GetRolesForUser(internalType);

            var externalResult = MessageTypeConverter.Instance.Convert<
                List<BookStore.Business.Entities.Role>,
                List<BookStore.Services.MessageTypes.Role>>(internalResult);

            return externalResult;
        }


        public List<Role> GetRolesForUserName(string pUserName)
        {
            var internalResult = RoleProvider.GetRolesForUserName(pUserName);
            var externalResult = MessageTypeConverter.Instance.Convert<
                    List<BookStore.Business.Entities.Role>,
                    List<BookStore.Services.MessageTypes.Role>>(internalResult);
            return externalResult;
        }
    }
}
