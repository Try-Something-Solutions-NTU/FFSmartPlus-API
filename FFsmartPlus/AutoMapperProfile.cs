using Application.Fridge;
using AutoMapper;
using Domain;

namespace FFsmartPlus;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Fridge, FridgeDto>();
        CreateMap<FridgeDto, Fridge>();
        // Use CreateMap... Etc.. here (Profile methods are the same as configuration methods)
    }
}