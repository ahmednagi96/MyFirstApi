using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs.Addresses.Responses;
using MyFirstApi.DTOs.Users.Requests;
using MyFirstApi.DTOs.Users.Responses;
using MyFirstApi.Entities;
using MyFirstApi.Exceptions;

namespace MyFirstApi.Services;

public class UserService(AppDbContext context) : IUserService
{
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllAsync(
        UserFilterDto filter,
        CancellationToken ct = default)
    {
        var query = context.Users
            .AsNoTracking()
            .Where(u => u.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(u => u.Name.Contains(filter.Search) || u.Email.Contains(filter.Search));

        if (!string.IsNullOrWhiteSpace(filter.Role))
            query = query.Where(u => u.Role == filter.Role);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(u => u.Status == filter.Status);

        return await query
            .OrderBy(u => u.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => new UserResponseDto(u.Id, u.Name, u.Email, u.Role, u.Status))
            .ToListAsync(ct);
    }

    public async Task<UserDetailsDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == id && u.DeletedAt == null)
            .Select(u => new UserDetailsDto(
                u.Id, u.Name, u.Email, u.Role, u.Status,
                u.CreatedAt, u.UpdatedAt, u.DeletedAt, u.PrimaryAddresses.Select(a => new AddressResponseDto(a.Street, a.City, a.Country)).ToList()))
            .FirstOrDefaultAsync(ct);

        return user ?? throw new KeyNotFoundException($"User with id {id} was not found.");
    }

    public async Task<UserDetailsDto> CreateAsync(CreateUserRequestDto dto, CancellationToken ct = default)
    {
        var emailTaken = await context.Users
            .AnyAsync(u => u.Email == dto.Email && u.DeletedAt == null, ct);

        if (emailTaken)
            throw new ConflictException($"Email '{dto.Email}' is already in use.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Role = dto.Role,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(ct);

        return ToDetails(user);
    }

    public async Task UpdateAsync(int id, UpdateUserRequestDto dto, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, ct)
            ?? throw new KeyNotFoundException($"User with id {id} was not found.");

        var emailTaken = await context.Users
            .AnyAsync(u => u.Email == dto.Email && u.Id != id && u.DeletedAt == null, ct);

        if (emailTaken)
            throw new ConflictException($"Email '{dto.Email}' is already in use.");

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.Role = dto.Role;
        user.Status = dto.Status;
        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, ct)
            ?? throw new KeyNotFoundException($"User with id {id} was not found.");

        user.DeletedAt = DateTime.UtcNow;
        user.Status = "Deleted";
        await context.SaveChangesAsync(ct);
    }

    private static UserDetailsDto ToDetails(User user) => new UserDetailsDto(
        user.Id,
        user.Name,
        user.Email,
        user.Role,
        user.Status,
        user.CreatedAt,
        user.UpdatedAt,
        user.DeletedAt,
        user.PrimaryAddresses.Select(a => new AddressResponseDto(a.Street, a.City, a.Country)).ToList()
        );
}
