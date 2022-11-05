using Application.Fridge;
using Application.Item;
using Application.Unit;
using AutoMapper;
using Domain;
using Unit = MediatR.Unit;

namespace FFsmartPlus;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<UnitDto, Domain.Unit>();
        CreateMap<Domain.Unit, UnitDto>();
        CreateMap<UnitListDto, Domain.Unit>();
        CreateMap<Domain.Unit, UnitListDto>();
        CreateMap<Item, ItemDto>();
        CreateMap<ItemDto, Item>();
        CreateMap<NewItemDto, Item>();
        // Use CreateMap... Etc.. here (Profile methods are the same as configuration methods)
    }
}