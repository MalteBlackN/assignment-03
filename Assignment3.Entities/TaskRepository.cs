using static Assignment3.Core.State;

namespace Assignment3.Entities;

public class TaskRepository : ITaskRepository
{
    private readonly KanbanContext context;

    public TaskRepository(KanbanContext _context)
    {
        context = _context;
    }

    public (Response Response, int TaskId) Create(TaskCreateDTO task)
    {
        var entity = context.Tasks.FirstOrDefault(c => c.Title == task.Title);
        Response response;

        if (entity is null)
        {
            entity = new Task { Title = task.Title };

            context.Tasks.Add(entity);
            context.SaveChanges();

            response = Created;
        }
        else
        {
            response = Conflict;
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
            select new TaskDetailsDTO(c.Id, c.Title, c.Description, new DateTime(2020, 01, 01), c.AssignedTo.Name, (IReadOnlyCollection<string>)c.Tags.Select(c => c.Name), c.State, new DateTime(2022, 01, 01));

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
        else
        {
            entity.Title = task.Title;
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
        else
        {
            context.Tasks.Remove(task);
            context.SaveChanges();

            response = Deleted;
        }

        return response;
    }
}
