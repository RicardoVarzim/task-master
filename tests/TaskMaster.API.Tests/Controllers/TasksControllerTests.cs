using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.API.Controllers;
using TaskMaster.API.Models;
using TaskMaster.Core.Data;
using TaskMaster.Core.Models;
using TaskMaster.Core.Services;
using TaskModel = TaskMaster.Core.Models.Task;
using TaskStatusModel = TaskMaster.Core.Models.TaskStatus;
using Xunit;

namespace TaskMaster.API.Tests.Controllers;

public class TasksControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TasksController _controller;
    private readonly ITaskParsingService _parsingService;

    public TasksControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _parsingService = new TaskParsingService();
        _controller = new TasksController(_context, _parsingService);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var project = new Project
        {
            Id = 1,
            Name = "Test Project",
            FullPath = "C:\\Test\\Project",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);

        var tasks = new List<TaskModel>
        {
            new TaskModel
            {
                Id = 1,
                Description = "Test Task 1",
                IsCompleted = false,
                ProjectId = 1,
                Status = TaskStatusModel.Pending,
                Priority = TaskPriority.Medium,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SourceFilePath = "C:\\Test\\Project\\tasks.md",
                LineNumber = 1
            },
            new TaskModel
            {
                Id = 2,
                Description = "Test Task 2",
                IsCompleted = true,
                ProjectId = 1,
                Status = TaskStatusModel.Completed,
                Priority = TaskPriority.High,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                SourceFilePath = "C:\\Test\\Project\\tasks.md",
                LineNumber = 2
            }
        };

        _context.Tasks.AddRange(tasks);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetTasks_ReturnsAllTasks()
    {
        // Act
        var result = await _controller.GetTasks();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var tasks = Assert.IsType<PagedResult<TaskModel>>(okResult.Value);
        
        Assert.Equal(2, tasks.TotalCount);
        Assert.Equal(2, tasks.Items.Count);
    }

    [Fact]
    public async Task GetTasks_WithProjectIdFilter_ReturnsFilteredTasks()
    {
        // Act
        var result = await _controller.GetTasks(projectId: 1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var tasks = Assert.IsType<PagedResult<TaskModel>>(okResult.Value);
        
        Assert.Equal(2, tasks.TotalCount);
        Assert.All(tasks.Items, t => Assert.Equal(1, t.ProjectId));
    }

    [Fact]
    public async Task GetTasks_WithIsCompletedFilter_ReturnsFilteredTasks()
    {
        // Act
        var result = await _controller.GetTasks(isCompleted: true);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var tasks = Assert.IsType<PagedResult<TaskModel>>(okResult.Value);
        
        Assert.Equal(1, tasks.TotalCount);
        Assert.All(tasks.Items, t => Assert.True(t.IsCompleted));
    }

    [Fact]
    public async Task GetTasks_WithPagination_ReturnsPagedResults()
    {
        // Act
        var result = await _controller.GetTasks(pageNumber: 1, pageSize: 1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var tasks = Assert.IsType<PagedResult<TaskModel>>(okResult.Value);
        
        Assert.Equal(2, tasks.TotalCount);
        Assert.Equal(1, tasks.Items.Count);
        Assert.Equal(2, tasks.TotalPages);
        Assert.True(tasks.HasNextPage);
    }

    [Fact]
    public async Task GetTask_WithValidId_ReturnsTask()
    {
        // Act
        var result = await _controller.GetTask(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var task = Assert.IsType<TaskModel>(okResult.Value);
        
        Assert.Equal(1, task.Id);
        Assert.Equal("Test Task 1", task.Description);
    }

    [Fact]
    public async Task GetTask_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetTask(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetTasks_WithSorting_ReturnsSortedTasks()
    {
        // Act
        var result = await _controller.GetTasks(sortBy: "priority", sortOrder: "asc");

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var tasks = Assert.IsType<PagedResult<TaskModel>>(okResult.Value);
        
        Assert.Equal(2, tasks.Items.Count);
        // First task should have lower priority (Medium < High when nulls are handled)
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

