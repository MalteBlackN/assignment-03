namespace Assignment3.Entities.Tests;

public class TaskRepositoryTests
{
    private readonly KanbanContext context;
    private readonly TaskRepository repository;

    public TaskRepositoryTests()
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
        var task2 = new Task { Id = 2, Title = "Refill storage", Tags = new List<Tag>(){tag2}, State = Active };
        var task3 = new Task { Id = 3, Title = "Refill storage", Tags = new List<Tag>(){tag2}, State = Resolved };
        var task4 = new Task { Id = 4, Title = "Refill storage", Tags = new List<Tag>(){tag2}, State = Closed };
        var task5 = new Task { Id = 5, Title = "Refill storage", Tags = new List<Tag>(){tag2}, State = Removed };

        var user1 = new User { Id = 1, Name = "Gert", Email = "gert@google.com", Tasks = new List<Task>(){}};

        tag1.Tasks.Add(task1);
        tag2.Tasks.Add(task2);
        tag2.Tasks.Add(task3);
        tag2.Tasks.Add(task4);
        tag2.Tasks.Add(task5);

        _context.Tags.AddRange(tag1, tag2);
        _context.Tasks.AddRange(task1, task2, task3, task4, task5);
        _context.Users.Add(user1);
        _context.SaveChanges();

        context = _context;
        repository = new TaskRepository(context);
    }

    [Fact]
    public void Delete_New_returns_Deleted()
    {
        // Act
        var response = repository.Delete(1);

        // Assert
        response.Should().Be(Deleted);
    }

    [Fact]
    public void Delete_New_Changes_State_to_Removed()
    {
        // Act
        var response = repository.Delete(1);
        var state = repository.Read(1).State;

        // Assert
        response.Should().Be(Deleted);
        state.Should().Be(Removed);
    }

    [Fact]
    public void Delete_not_New_returns_BadRequest()
    {
        // Act
        var response = repository.Delete(2);

        // Assert
        response.Should().Be(BadRequest);
    }

    [Fact]
    public void Delete_Resolved_Closed_Removed_returns_Conflict()
    {
        // Act
        var response1 = repository.Delete(3);
        var response2 = repository.Delete(4);
        var response3 = repository.Delete(5);

        // Assert
        response1.Should().Be(Conflict);
        response2.Should().Be(Conflict);
        response3.Should().Be(Conflict);
    }

    [Fact]
    public void Create_Task_sets_State_to_New()
    {
        // Act
        var response = repository.Create(new TaskCreateDTO("Organize shelfs", 1, null, new List<string>()));
        var state = repository.Read(response.TaskId).State;

        // Assert
        state.Should().Be(New);
    }

    [Fact]
    public void Create_Task_sets_Created_and_StateUpdated_to_current_time()
    {
        // Arrange
        var expected = DateTime.UtcNow;

        // Act
        var response = repository.Create(new TaskCreateDTO("Organize shelfs", 1, null, new List<string>()));
        var actualCreated = repository.Read(response.TaskId).Created;
        var actualStateUpdated = repository.Read(response.TaskId).StateUpdated;

        // Assert
        response.Response.Should().Be(Created);
        actualCreated.Should().BeCloseTo(expected, precision: TimeSpan.FromSeconds(5));
        actualStateUpdated.Should().BeCloseTo(expected, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_State_updates_StateUpdated()
    {
        // Arrange
        var response = repository.Create(new TaskCreateDTO("Organize shelfs", 1, null, new List<string>()));
        Thread.Sleep(7000);
        var expected = DateTime.UtcNow;

        // Act
        var task = repository.Read(response.TaskId);
        repository.Update(new TaskUpdateDTO(response.TaskId, task.Title, 1, task.Description, new List<string>(), Active));
        var actual = repository.Read(response.TaskId).StateUpdated;

        // Assert
        actual.Should().BeCloseTo(expected, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_Assigning_Non_Existing_User_Returns_BadRequest()
    {
        // Act
        var response = repository.Create(new TaskCreateDTO("Organize shelfs", 1001, "The shelfs must be organized neatly", new List<string>()));

        // Assert
        response.Response.Should().Be(BadRequest);
    }

    [Fact]
    public void Update_Assigning_Non_Existing_User_Returns_BadRequest()
    {
        // Arrange
        var task = context.Tasks.Find(1);

        // Act
        var response = repository.Update(new TaskUpdateDTO(task!.Id, task.Title, 1001, task.Description, task.Tags.Select(c => c.Name).ToList(), task.State));

        // Assert
        response.Should().Be(BadRequest);
    }

    [Fact]
    public void Create_Allows_Assigning_Tags()
    {
        // Arrange
        var response = repository.Create(new TaskCreateDTO("Clean windows", null, "The windows must be clear", new List<string>(){context.Tags.Find(1)!.Name}));

        // Assert
        context.Tasks.Find(response.TaskId)!.Tags.Should().BeEquivalentTo(new List<Tag>(){context.Tags.Find(1)!});
    }

    [Fact]
    public void Update_Allows_Changing_Tags()
    {
        // Arrange
        context.Tasks.Find(1)!.Tags.Should().BeEquivalentTo(new List<Tag>(){context.Tags.Find(1)!});
        var newTag = new Tag { Id = 3, Name = "Organize", Tasks = new List<Task>() };
        context.Tags.Add(newTag);
        context.SaveChanges();

        // Act
        var task = repository.Read(1);
        var response = repository.Update(new TaskUpdateDTO(1, task.Title, null, null, new List<string>(){newTag.Name}, task.State));

        // Assert
        response.Should().Be(Updated);
        Assert.Equivalent(new List<Tag>(){newTag}, context.Tasks.Find(1)!.Tags);
    }
}
