namespace Assignment3.Entities;

public class User
{
    public int Id { get;init; }
    public string Name { get;set; }
    public string Email { get;set; }
    public List<Task> Tasks { get;set; }
}
