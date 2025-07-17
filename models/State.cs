
namespace WorkflowEngine.Models;

public class State //for creating states in workflow definitions
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsInitial { get; set; }
    public bool IsFinal { get; set; }
    public string? Description { get; set; }
}