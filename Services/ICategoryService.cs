using MyFirstApi.DTOs.Categories.Requests;
using MyFirstApi.DTOs.Categories.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MyFirstApi.Services;

public interface  ICategoryService
{
    Task<ActionResult<IEnumerable<CategoryDetailsDto>>> GetAllAsync(CancellationToken ct = default);
    Task<CategoryDetailsDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CategoryDetailsDto> CreateAsync(CreateCategoryRequestDto dto, CancellationToken ct = default);
   
}
