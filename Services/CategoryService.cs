using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs.Categories.Requests;
using MyFirstApi.DTOs.Categories.Responses;
using MyFirstApi.Entities;
using MyFirstApi.Exceptions;

namespace MyFirstApi.Services;

public class CategoryService(AppDbContext context,IMapper mapper) :  ICategoryService
{
    public async Task<ActionResult<IEnumerable<CategoryDetailsDto>>> GetAllAsync(CancellationToken ct = default)    {
        var categories = await context.Categories
        .AsNoTracking()
        .IgnoreQueryFilters()
        .ProjectTo<CategoryDetailsDto>(mapper.ConfigurationProvider)
        .ToListAsync(ct);

        return categories;
    }

    public async Task<CategoryDetailsDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var category = await context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .IgnoreQueryFilters()
            .ProjectTo<CategoryDetailsDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

        return category ?? throw new KeyNotFoundException($"Category with id {id} was not found.");
    }

    public async Task<CategoryDetailsDto> CreateAsync(CreateCategoryRequestDto dto, CancellationToken ct = default)
    {
        var nameTaken = await context.Categories
            .AnyAsync(c => c.Name == dto.Name && c.DeletedAt == null, ct);

        if (nameTaken)
            throw new ConflictException($"Name '{dto.Name}' is already in use.");

        var category =mapper.Map<Category>(dto);
        context.Categories.Add(category);
        await context.SaveChangesAsync(ct);

        return ToDetails(category);
    }

  

   
    private CategoryDetailsDto ToDetails(Category category) => mapper.Map<CategoryDetailsDto>(category);
}
