namespace BAI.ConsoleApp;

public class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public string Description { get; set; } = string.Empty;
}
