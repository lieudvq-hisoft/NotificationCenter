using AutoMapper;
using Data.Entities;
using Data.Models;

namespace Data.Mapping
{
	public class MapperProfile : Profile
    {
		public MapperProfile()
		{
            CreateMap<Notification, NotificationModel>().ReverseMap();

            CreateMap<User, UserModel>().ReverseMap();
            CreateMap<UserFromKafka, User>().ReverseMap();
        }
	}
}

