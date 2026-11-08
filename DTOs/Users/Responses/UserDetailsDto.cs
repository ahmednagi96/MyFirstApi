using MyFirstApi.DTOs.Addresses.Responses;
namespace MyFirstApi.DTOs.Users.Responses;

public record UserDetailsDto(
    int Id,
    string Name,
    string Email,
    string Role,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeletedAt,

    List<AddressResponseDto>? PrimaryAddresses
    );
