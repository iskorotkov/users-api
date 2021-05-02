using AutoMapper;
using Models.Entities;
using WebApi.DTOs;

namespace WebApi.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<User, UserGetDto>();
            CreateMap<UserGroup, UserGroupGetDto>();
            CreateMap<UserState, UserStateGetDto>();
        }
    }
}
