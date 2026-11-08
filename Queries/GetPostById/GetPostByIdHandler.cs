using MediatR;
using MyFirstApi.DTOs.Posts.Responses;
using MyFirstApi.Repositories;

namespace MyFirstApi.Queries.GetPostById;

public class GetPostByIdHandler(IPostRepository postRepository) : IRequestHandler<GetPostByIdQuery, PostDetailsDto>
{
    public async Task<PostDetailsDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetDetailsByIdAsync(request.Id, cancellationToken);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with id {request.Id} was not found.");
        }

        return post;
    }
}
