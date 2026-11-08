namespace MyFirstApi.DTOs.Addresses.Responses;

public record AddressResponseDto(
    string? Street,
    string? City,
    string? Country
);
