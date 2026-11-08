using AutoMapper;
using MyFirstApi.DTOs.Categories.Requests;
using MyFirstApi.DTOs.Categories.Responses;
using MyFirstApi.DTOs.Posts.Responses;
using MyFirstApi.DTOs.Tools.Responses;
using MyFirstApi.DTOs.Users.Responses;
using MyFirstApi.Entities;

namespace MyFirstApi.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateCategoryRequestDto, Category>();
        CreateMap<Category, CategoryDetailsDto>();
        CreateMap<Tool, ToolDetailsDto>();
        CreateMap<User, UserResponseDto>();
        CreateMap<Post, PostDetailsDto>()
            .ForCtorParam(nameof(PostDetailsDto.Author), opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.TimeAgo, opt => opt.Ignore());
    }
}
