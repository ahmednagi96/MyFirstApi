using MyFirstApi.Entities;
using MyFirstApi.DTOs.Posts.Responses;

namespace MyFirstApi.Repositories;

public interface IPostRepository
{
    Task<PostDetailsDto?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
