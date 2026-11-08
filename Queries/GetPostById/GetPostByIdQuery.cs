using MediatR;
using MyFirstApi.DTOs.Posts.Responses;
namespace MyFirstApi.Queries.GetPostById;

public record GetPostByIdQuery(int Id) : IRequest<PostDetailsDto>;

