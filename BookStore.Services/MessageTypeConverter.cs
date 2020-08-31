using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Components;

namespace BookStore.Services
{
    /// <summary>
    /// Based on AutoMapper: https://github.com/AutoMapper/AutoMapper/wiki/Getting-started
    /// See above link if help is needed
    /// </summary>
    public class MessageTypeConverter
    {
        private static MessageTypeConverter sMessageTypeConverter = new MessageTypeConverter();

        public static MessageTypeConverter Instance
        {
            get
            {
                return sMessageTypeConverter;
            }
        }



        public MessageTypeConverter()
        {
            InitializeExternalToInternalMappings();
            InitializeInternalToExternalMappings();
        }

        private void InitializeInternalToExternalMappings()
        {
            AutoMapper.Mapper.CreateMap<BookStore.Business.Entities.Book,
                                        BookStore.Services.MessageTypes.Book>().ForMember(
                dest => dest.StockCount, opts => opts.MapFrom(src => ServiceFactory.GetService<ICatalogueProvider>().getBookStock(src.Id)));

            AutoMapper.Mapper.CreateMap<BookStore.Business.Entities.Order,
                                        BookStore.Services.MessageTypes.Order>();

            AutoMapper.Mapper.CreateMap<BookStore.Business.Entities.OrderItem,
                                        BookStore.Services.MessageTypes.OrderItem>();

            AutoMapper.Mapper.CreateMap<BookStore.Business.Entities.User,
                                        BookStore.Services.MessageTypes.User>();

            AutoMapper.Mapper.CreateMap<BookStore.Business.Entities.LoginCredential,
                                        BookStore.Services.MessageTypes.LoginCredential>();
        }

        public void InitializeExternalToInternalMappings()
        {
            AutoMapper.Mapper.CreateMap<BookStore.Services.MessageTypes.Book,
                                        BookStore.Business.Entities.Book>();

            AutoMapper.Mapper.CreateMap<BookStore.Services.MessageTypes.Order,
                                        BookStore.Business.Entities.Order>();

            AutoMapper.Mapper.CreateMap<BookStore.Services.MessageTypes.OrderItem,
                                        BookStore.Business.Entities.OrderItem>();

            AutoMapper.Mapper.CreateMap<BookStore.Services.MessageTypes.User,
                                        BookStore.Business.Entities.User>();

            AutoMapper.Mapper.CreateMap<BookStore.Services.MessageTypes.LoginCredential,
                                        BookStore.Business.Entities.LoginCredential>();
        }

        public Destination Convert<Source, Destination>(Source s) where Destination : class
        {
            if(typeof(Source) == typeof(BookStore.Services.MessageTypes.User))
            {
                return ConvertUserToInternalType(s as MessageTypes.User) as Destination;
            }
            else if(typeof(Source) == typeof(BookStore.Business.Entities.User))
            {
                return ConvertToExternalType(s as Business.Entities.User) as Destination;
            }
            var result = AutoMapper.Mapper.Map<Source, Destination>(s);
            if(typeof(Source) == typeof(MessageTypes.Order))
            {
                (result as Business.Entities.Order).Customer = ConvertUserToInternalType(
                    (s as MessageTypes.Order).Customer
                );
            }
            return result;
        }

        private MessageTypes.User ConvertToExternalType(Business.Entities.User user)
        {
            MessageTypes.User lExternal = new MessageTypes.User()
            {
                Address = user.Address,
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                Revision = user.Revision,
            };

            if(user.LoginCredential != null)
            {
                lExternal.LoginCredential = new MessageTypes.LoginCredential()
                {
                    Id = user.LoginCredential.Id,
                    UserName = user.LoginCredential.UserName,
                    EncryptedPassword = user.LoginCredential.EncryptedPassword
                };
            }

            return lExternal;
        }

        private Business.Entities.User ConvertUserToInternalType(MessageTypes.User user)
        {

            Business.Entities.User lInternal = UserProvider.ReadUserById(user.Id);
            if(lInternal == null)
            {
                lInternal = new Business.Entities.User();
            }
            
            
            lInternal.Address = user.Address;
            lInternal.Email = user.Email;
            lInternal.Id = user.Id;
            lInternal.Name = user.Name;
            lInternal.Revision = user.Revision;
            
            if(user.LoginCredential != null)
            {
                lInternal.LoginCredential = new Business.Entities.LoginCredential()
                {
                    Id = user.LoginCredential.Id,
                    UserName = user.LoginCredential.UserName,
                    EncryptedPassword = user.LoginCredential.EncryptedPassword
                };
            }
            return lInternal;
        }


        private IUserProvider UserProvider
        {
            get
            {
                return ServiceFactory.GetService<IUserProvider>();
            }
        }
    }
}
