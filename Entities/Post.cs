namespace MyFirstApi.Entities;

public class Post : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; }=string.Empty;
    public string Content { get; set; }=string.Empty;
    public DateTime? CreatedAt { get; set; }= null;
    public DateTime? UpdatedAt { get; set; }=null;
    public DateTime? DeletedAt { get; set; }=null;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
