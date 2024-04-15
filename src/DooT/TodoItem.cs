namespace DooT;

public class TodoItem
{
    public int Id { get; internal init; }
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public string Description { get; set; } = string.Empty;
}
