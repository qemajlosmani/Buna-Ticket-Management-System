using AutoMapper;
using Btms.Data.Entities;
using Models.Account;

namespace Btms.API.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //Account mapping
            CreateMap<Account, AccountResponse>();
            CreateMap<Account, LoginResponse>();
            CreateMap<SignupRequest, Account>();
            CreateMap<CreateAccountRequest, Account>();
            CreateMap<UpdateRequest, Account>()
                .ForAllMembers(x => x.Condition(
                    (src, dest, prop) =>
                    {
                        // ignore null & empty string properties
                        if (prop == null) return false;
                        if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop)) return false;

                        // ignore null role
                        if (x.DestinationMember.Name == "Role" && src.role == null) return false;

                        return true;
                    }
                ));
        }

    }
}
