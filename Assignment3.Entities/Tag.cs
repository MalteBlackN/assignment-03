namespace Assignment3.Entities;

public class Tag
{
    public int Id { get;init; }
    public string Name { get;set; }
    public virtual ICollection<Task> Tasks { get;set; }
}
