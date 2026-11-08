using MediatR;
using MyFirstApi.DTOs.Posts.Responses;

namespace MyFirstApi.Commands;

public record CreatePostCommand(string Title, string Content) : IRequest<PostDetailsDto>;
