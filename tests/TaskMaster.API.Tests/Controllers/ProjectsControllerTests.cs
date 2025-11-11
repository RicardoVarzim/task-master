using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.API.Controllers;
using TaskMaster.Core.Data;
using TaskMaster.Core.Models;
using Xunit;

namespace TaskMaster.API.Tests.Controllers;

public class ProjectsControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProjectsController _controller;
    private readonly string _testProjectPath;

    public ProjectsControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new ProjectsController(_context);

        // Create a temporary directory for testing
        _testProjectPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testProjectPath);
    }

    [Fact]
    public async Task GetProjects_ReturnsAllProjects()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            FullPath = _testProjectPath,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProjects();

        // Assert
        var okResult = Assert.IsType<ActionResult<IEnumerable<Project>>>(result);
        var projects = Assert.IsType<List<Project>>(okResult.Value);
        
        Assert.Single(projects);
        Assert.Equal("Test Project", projects[0].Name);
    }

    [Fact]
    public async Task GetProject_WithValidId_ReturnsProject()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            FullPath = _testProjectPath,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProject(project.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<Project>>(result);
        var returnedProject = Assert.IsType<Project>(okResult.Value);
        
        Assert.Equal(project.Id, returnedProject.Id);
        Assert.Equal("Test Project", returnedProject.Name);
    }

    [Fact]
    public async Task GetProject_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetProject(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateProject_WithValidPath_CreatesProject()
    {
        // Arrange
        var request = new CreateProjectRequest { Path = _testProjectPath };

        // Act
        var result = await _controller.CreateProject(request);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var project = Assert.IsType<Project>(createdAtResult.Value);
        
        Assert.Equal(Path.GetFileName(_testProjectPath.TrimEnd('\\', '/')), project.Name);
        Assert.Equal(_testProjectPath, project.FullPath);
    }

    [Fact]
    public async Task CreateProject_WithInvalidPath_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateProjectRequest { Path = "C:\\NonExistent\\Path" };

        // Act
        var result = await _controller.CreateProject(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateProject_WithDuplicatePath_ReturnsConflict()
    {
        // Arrange
        var project = new Project
        {
            Name = "Existing Project",
            FullPath = _testProjectPath,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var request = new CreateProjectRequest { Path = _testProjectPath };

        // Act
        var result = await _controller.CreateProject(request);

        // Assert
        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteProject_WithValidId_DeletesProject()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            FullPath = _testProjectPath,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteProject(project.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        
        var deletedProject = await _context.Projects.FindAsync(project.Id);
        Assert.Null(deletedProject);
    }

    [Fact]
    public async Task DeleteProject_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.DeleteProject(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        
        if (Directory.Exists(_testProjectPath))
        {
            Directory.Delete(_testProjectPath, true);
        }
    }
}

