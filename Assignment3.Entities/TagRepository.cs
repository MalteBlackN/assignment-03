namespace Assignment3.Entities;

public class TagRepository : ITagRepository
{
    private readonly KanbanContext context;

    public TagRepository(KanbanContext _context)
    {
        this.context = _context;
    }

    public (Response Response, int TagId) Create(TagCreateDTO tag)
    {
        var entity = context.Tags.FirstOrDefault(c => c.Name == tag.Name);
        Response response;

        if (entity is null)
        {
            entity = new Tag { Name = tag.Name};

            context.Tags.Add(entity);
            context.SaveChanges();

            response = Created;
        }
        else
        {
            response = Conflict;
        }

        return (response, entity.Id);
    }

    public IReadOnlyCollection<TagDTO> ReadAll()
    {
        var tags = from c in context.Tags
            orderby c.Name
            select new TagDTO(c.Id, c.Name);
        
        return tags.ToArray();
    }

    public TagDTO Read(int tagId)
    {
        var tag = from c in context.Tags
            where c.Id == tagId
            select new TagDTO(c.Id, c.Name);

        return tag.FirstOrDefault()!;
    }

    public Response Update(TagUpdateDTO tag)
    {
        var entity = context.Tags.Find(tag.Id);
        Response response;

        if (entity is null)
        {
            response = NotFound;
        }
        else if (context.Tags.FirstOrDefault(c => c.Id != tag.Id && c.Name == tag.Name) != null)
        {
            response = Conflict;
        }
        else
        {
            entity.Name = tag.Name;
            context.SaveChanges();
            response = Updated;
        }

        return response;
    }

    public Response Delete(int tagId, bool force = false)
    {
        var tag = context.Tags.FirstOrDefault(c => c.Id == tagId);
        Response response;

        if (tag is null)
        {
            response = NotFound;
        }
        else if (!force && tag.Tasks.Any())
        {
            response = Conflict;
        }
        else
        {
            context.Tags.Remove(tag);
            context.SaveChanges();

            response = Deleted;
        }

        return response;
    }
}
