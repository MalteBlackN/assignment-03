using static Assignment3.Core.State;

namespace Assignment3.Entities;

public class TaskRepository : ITaskRepository
{
    private readonly KanbanContext context;

    public TaskRepository(KanbanContext _context)
    {
        context = _context;
    }

    private ICollection<Tag> Convert_Strings_to_Tags(ICollection<string> strings)
    {
        ICollection<Tag> tags = new List<Tag>();
        foreach (string s in strings)
        {
            foreach (Tag t in context.Tags)
            {
                if (t.Name.Equals(s))
                {
                    tags.Add(t);
                }

            }
        }
        return tags;
    }

    public (Response Response, int TaskId) Create(TaskCreateDTO task)
    {
        var entity = context.Tasks.FirstOrDefault(c => c.Title == task.Title);
        Response response;
        ICollection<Tag> tags;
        if (task.Tags is null)
        {
            tags = new List<Tag>();
        }
        else
        {
            tags = Convert_Strings_to_Tags(task.Tags);
        }

        if (entity is not null)
        {
            response = Conflict;
        }
        else if (task.AssignedToId is not null && context.Users.Find(task.AssignedToId) is null)
        {
            response = BadRequest;
            entity = new Task { Title = task.Title, Description = task.Description!, State = New, Tags = tags, Created = DateTime.UtcNow, StateUpdated = DateTime.UtcNow };
        }
        else
        {
            entity = new Task { Title = task.Title, Description = task.Description!, State = New, Tags = tags, Created = DateTime.UtcNow, StateUpdated = DateTime.UtcNow };

            context.Tasks.Add(entity);
            context.SaveChanges();

            response = Created;
        }

        return (response, entity.Id);
    }

    public IReadOnlyCollection<TaskDTO> ReadAll()
    {
        var tasks = from c in context.Tasks
            orderby c.Title
            select new TaskDTO(c.Id, c.Title, c.AssignedTo.Name, (IReadOnlyCollection<string>)c.Tags.Select(c => c.Name), c.State);
        
        return tasks.ToArray();
    }

    public IReadOnlyCollection<TaskDTO> ReadAllRemoved()
    {
        var tasks = from c in context.Tasks
            where c.State == Removed
            orderby c.Title
            select new TaskDTO(c.Id, c.Title, c.AssignedTo.Name, (IReadOnlyCollection<string>)c.Tags.Select(c => c.Name), c.State);
        
        return tasks.ToArray();
    }

    public IReadOnlyCollection<TaskDTO> ReadAllByTag(string tag)
    {
        var tasks = from c in context.Tasks
            where ((IReadOnlyCollection<string>)c.Tags.Select(c => c.Name)).Contains(tag)
            orderby c.Title
            select new TaskDTO(c.Id, c.Title, c.AssignedTo.Name, (IReadOnlyCollection<string>)c.Tags.Select(c => c.Name), c.State);
        
        return tasks.ToArray();
    }

    public IReadOnlyCollection<TaskDTO> ReadAllByUser(int userId)
    {
        var tasks = from c in context.Tasks
            where c.AssignedTo.Id == userId
            orderby c.Title
            select new TaskDTO(c.Id, c.Title, c.AssignedTo.Name, (IReadOnlyCollection<string>)c.Tags.Select(c => c.Name), c.State);
        
        return tasks.ToArray();
    }

    public IReadOnlyCollection<TaskDTO> ReadAllByState(State state)
    {
        var tasks = from c in context.Tasks
            where c.State == state
            orderby c.Title
            select new TaskDTO(c.Id, c.Title, c.AssignedTo.Name, (IReadOnlyCollection<string>)c.Tags.Select(c => c.Name), c.State);
        
        return tasks.ToArray();
    }

    public TaskDetailsDTO Read(int taskId)
    {
        var task = from c in context.Tasks
            where c.Id == taskId
            select new TaskDetailsDTO(c.Id, c.Title, c.Description, c.Created, c.AssignedTo.Name, (IReadOnlyCollection<string>)c.Tags.Select(c => c.Name), c.State, c.StateUpdated);

        return task.FirstOrDefault()!;
    }

    public Response Update(TaskUpdateDTO task)
    {
        var entity = context.Tasks.Find(task.Id);
        Response response;

        if (entity is null)
        {
            response = NotFound;
        }
        else if (context.Tasks.FirstOrDefault(c => c.Id != task.Id && c.Title == task.Title) != null)
        {
            response = Conflict;
        }
        else if (task.AssignedToId is not null && context.Users.Find(task.AssignedToId) is null)
        {
            response = BadRequest;
        }
        else
        {
            entity.Title = task.Title;
            if (task.Description != null)
                entity.Description = task.Description;
            if (task.AssignedToId is not null)
            {
                entity.AssignedTo = context.Users.Find(task.AssignedToId);
            }
            if (task.Tags is not null)
            {
                entity.Tags = Convert_Strings_to_Tags(task.Tags);
            }
            if (entity.State != task.State)
            {
                entity.State = task.State;
                entity.StateUpdated = DateTime.UtcNow;
            }

            context.SaveChanges();
            response = Updated;
        }

        return response;
    }

    public Response Delete(int taskId)
    {
        var task = context.Tasks.FirstOrDefault(c => c.Id == taskId);
        Response response;

        if (task is null)
        {
            response = NotFound;
        }
        else if (task.State == Active)
        {
            response = BadRequest;
        }
        else if (task.State == Resolved || task.State == Closed || task.State == Removed)
        {
            response = Conflict;
        }
        else
        {
            task.State = Removed;
            context.SaveChanges();

            response = Deleted;
        }

        return response;
    }
}
