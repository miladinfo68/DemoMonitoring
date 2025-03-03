namespace Demo.Shared.Models;

public class Todo
{
    public Todo() { } // Parameterless constructor added

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}
