using MyFirstApi.DTOs.Users.Requests;
using MyFirstApi.DTOs.Users.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MyFirstApi.Services;

public interface IUserService
{
    Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllAsync(UserFilterDto filter, CancellationToken ct = default);
    Task<UserDetailsDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<UserDetailsDto> CreateAsync(CreateUserRequestDto dto, CancellationToken ct = default);
    Task UpdateAsync(int id, UpdateUserRequestDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
