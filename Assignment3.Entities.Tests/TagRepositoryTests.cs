namespace Assignment3.Entities.Tests;

public class TagRepositoryTests : IDisposable
{
    private readonly KanbanContext context;
    private readonly TagRepository repository;

    public TagRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var _context = new KanbanContext(builder.Options);
        _context.Database.EnsureCreated();
        var tag1 = new Tag { Id = 1, Name = "Cleaning", Tasks = new List<Task>() };
        var tag2 = new Tag { Id = 2, Name = "Refill", Tasks = new List<Task>() };
        var task1 = new Task { Id = 1, Title = "Clean floors", Tags = new List<Tag>(){tag1}, State = New };
        tag1.Tasks.Add(task1);

        _context.Tags.AddRange(tag1, tag2);
        _context.Tasks.Add(task1);
        _context.SaveChanges();

        context = _context;
        repository = new TagRepository(context);
    }

    [Fact]
    public void Create_given_Tag_returns_Created_with_Tag()
    {
        // Arrange
        var (response, created) = repository.Create(new TagCreateDTO("Program"));

        // Assert
        response.Should().Be(Created);
        created.Should().Be(new TagDTO(3, "Program").Id);
    }

    [Fact]
    public void Create_given_existing_Tag_returns_Conflict_with_existing_Tag()
    {
        // Arrange
        var (response, tag) = repository.Create(new TagCreateDTO("Cleaning"));

        // Act
        response.Should().Be(Conflict);

        // Assert
        tag.Should().Be(new TagDTO(1, "Cleaning").Id);
    }

    [Fact]
    public void Find_given_non_existing_id_returns_null() => repository.Read(42).Should().BeNull();

    [Fact]
    public void Find_given_existing_id_returns_tag() => repository.Read(2).Should().Be(new TagDTO(2, "Refill"));

    [Fact]
    public void ReadAll_returns_all_tags() => repository.ReadAll().Should().BeEquivalentTo(new[] { new TagDTO(1, "Cleaning"), new TagDTO(2, "Refill") });

    [Fact]
    public void Update_given_non_existing_Tag_returns_NotFound() => repository.Update(new TagUpdateDTO(42, "Tidying")).Should().Be(NotFound);

    [Fact]
    public void Update_given_existing_name_returns_Conflict_and_does_not_update()
    {
        var response = repository.Update(new TagUpdateDTO(2, "Cleaning"));

        response.Should().Be(Conflict);

        var entity = context.Tags.Find(2)!;

        entity.Name.Should().Be("Refill");
    }

    [Fact]
    public void Update_updates_and_returns_Updated()
    {
        var response = repository.Update(new TagUpdateDTO(2, "Tidying"));

        response.Should().Be(Updated);

        var entity = context.Tags.Find(2)!;

        entity.Name.Should().Be("Tidying");
    }

    [Fact]
    public void Delete_given_non_existing_Id_returns_NotFound() => repository.Delete(42).Should().Be(NotFound);

    [Fact]
    public void Delete_deletes_and_returns_Deleted()
    {
        var response = repository.Delete(2);

        response.Should().Be(Deleted);

        var entity = context.Tags.Find(2);

        entity.Should().BeNull();
    }

    [Fact]
    public void Delete_given_existing_Tag_with_Tasks_returns_Conflict_and_does_not_delete()
    {
        // Act
        var response = repository.Delete(1);

        // Assert
        response.Should().Be(Conflict);
        context.Tags.Find(1).Should().NotBeNull();
    }

    [Fact]
    public void Delete_given_existing_Tag_with_Tasks_and_Force_equals_True_Deletes_and_returns_deleted()
    {
        // Act
        var response = repository.Delete(1, true);

        // Assert
        response.Should().Be(Deleted);
        context.Tags.Find(1).Should().BeNull();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}
