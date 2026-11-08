namespace MyFirstApi.Entities;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
}
