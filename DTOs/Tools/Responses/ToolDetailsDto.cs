namespace MyFirstApi.DTOs.Tools.Responses;

public record ToolDetailsDto(int Id,string Name)
{
    public string Developer => "Ahmed Nagi";
}
