using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs.Posts.Responses;
using MyFirstApi.Entities;

namespace MyFirstApi.Repositories;

public class PostRepository(AppDbContext context, IMapper mapper) : IPostRepository
{
    public async Task<PostDetailsDto?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Posts
            .AsNoTracking()
            .Where(p => p.Id == id)
            .ProjectTo<PostDetailsDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        await context.Posts.AddAsync(post, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (post is null)
            {
                throw new KeyNotFoundException($"Post with id {id} was not found.");
            }
            context.Posts.Remove(post);
        }
}