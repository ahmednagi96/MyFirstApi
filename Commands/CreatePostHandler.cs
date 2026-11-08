using MediatR;
using MyFirstApi.DTOs.Posts.Responses;
using MyFirstApi.Entities;
using MyFirstApi.Repositories;
using MyFirstApi.Services;

namespace MyFirstApi.Commands;

public class CreatePostHandler(IPostRepository postRepository, ICurrentUser currentUser)
    : IRequestHandler<CreatePostCommand, PostDetailsDto>
{
    public async Task<PostDetailsDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            UserId = currentUser.GetRequiredUserId()
        };

        await postRepository.AddAsync(post, cancellationToken);
        await postRepository.SaveChangesAsync(cancellationToken);

        return await postRepository.GetDetailsByIdAsync(post.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Post with id {post.Id} was not found.");
    }
}
