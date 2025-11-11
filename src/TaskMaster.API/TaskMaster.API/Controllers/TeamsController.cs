using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Core.Data;
using TaskMaster.Core.Models;

namespace TaskMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeamsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all teams, optionally filtered by project
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Team>>> GetTeams([FromQuery] int? projectId = null)
    {
        var query = _context.Teams
            .Include(t => t.Project)
            .Include(t => t.Members)
            .AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        var teams = await query.ToListAsync();
        return Ok(teams);
    }

    /// <summary>
    /// Gets a specific team by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Team>> GetTeam(int id)
    {
        var team = await _context.Teams
            .Include(t => t.Project)
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound();
        }

        return Ok(team);
    }

    /// <summary>
    /// Creates a new team
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Team>> CreateTeam([FromBody] CreateTeamRequest request)
    {
        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project == null)
        {
            return NotFound($"Project with ID {request.ProjectId} not found");
        }

        var team = new Team
        {
            Name = request.Name,
            ProjectId = request.ProjectId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTeam),
            new { id = team.Id },
            team);
    }

    /// <summary>
    /// Updates an existing team
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTeam(int id, [FromBody] UpdateTeamRequest request)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        team.Name = request.Name;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Deletes a team
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Gets all members of a team
    /// </summary>
    [HttpGet("{teamId}/members")]
    public async Task<ActionResult<IEnumerable<TeamMember>>> GetTeamMembers(int teamId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
        {
            return NotFound($"Team with ID {teamId} not found");
        }

        var members = await _context.TeamMembers
            .Where(m => m.TeamId == teamId)
            .Include(m => m.Team)
            .ToListAsync();

        return Ok(members);
    }

    /// <summary>
    /// Adds a member to a team
    /// </summary>
    [HttpPost("{teamId}/members")]
    public async Task<ActionResult<TeamMember>> AddTeamMember(int teamId, [FromBody] AddMemberRequest request)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
        {
            return NotFound($"Team with ID {teamId} not found");
        }

        var member = new TeamMember
        {
            Name = request.Name,
            Email = request.Email,
            GitUsername = request.GitUsername,
            TeamId = teamId,
            CreatedAt = DateTime.UtcNow
        };

        _context.TeamMembers.Add(member);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTeamMembers),
            new { teamId = teamId },
            member);
    }

    /// <summary>
    /// Updates a team member
    /// </summary>
    [HttpPut("members/{memberId}")]
    public async Task<IActionResult> UpdateTeamMember(int memberId, [FromBody] UpdateMemberRequest request)
    {
        var member = await _context.TeamMembers.FindAsync(memberId);
        if (member == null)
        {
            return NotFound($"Team member with ID {memberId} not found");
        }

        member.Name = request.Name;
        member.Email = request.Email;
        member.GitUsername = request.GitUsername;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Removes a member from a team
    /// </summary>
    [HttpDelete("members/{memberId}")]
    public async Task<IActionResult> RemoveTeamMember(int memberId)
    {
        var member = await _context.TeamMembers.FindAsync(memberId);
        if (member == null)
        {
            return NotFound($"Team member with ID {memberId} not found");
        }

        _context.TeamMembers.Remove(member);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Gets task assignments for a team
    /// </summary>
    [HttpGet("{teamId}/assignments")]
    public async Task<ActionResult<IEnumerable<TaskAssignment>>> GetTeamAssignments(int teamId)
    {
        var team = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            return NotFound($"Team with ID {teamId} not found");
        }

        var memberIds = team.Members.Select(m => m.Id).ToList();
        var assignments = await _context.TaskAssignments
            .Where(a => memberIds.Contains(a.TeamMemberId))
            .Include(a => a.Task)
            .ThenInclude(t => t.Project)
            .Include(a => a.TeamMember)
            .ToListAsync();

        return Ok(assignments);
    }

    /// <summary>
    /// Assigns a task to a team member
    /// </summary>
    [HttpPost("assignments")]
    public async Task<ActionResult<TaskAssignment>> AssignTask([FromBody] AssignTaskRequest request)
    {
        var task = await _context.Tasks.FindAsync(request.TaskId);
        if (task == null)
        {
            return NotFound($"Task with ID {request.TaskId} not found");
        }

        var member = await _context.TeamMembers.FindAsync(request.TeamMemberId);
        if (member == null)
        {
            return NotFound($"Team member with ID {request.TeamMemberId} not found");
        }

        // Check if assignment already exists
        var existingAssignment = await _context.TaskAssignments
            .FirstOrDefaultAsync(a => a.TaskId == request.TaskId && a.TeamMemberId == request.TeamMemberId);

        if (existingAssignment != null)
        {
            // Update existing assignment
            existingAssignment.Role = request.Role;
            existingAssignment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(existingAssignment);
        }

        var assignment = new TaskAssignment
        {
            TaskId = request.TaskId,
            TeamMemberId = request.TeamMemberId,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TaskAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTeamAssignments),
            new { teamId = member.TeamId },
            assignment);
    }

    /// <summary>
    /// Removes a task assignment
    /// </summary>
    [HttpDelete("assignments/{assignmentId}")]
    public async Task<IActionResult> RemoveAssignment(int assignmentId)
    {
        var assignment = await _context.TaskAssignments.FindAsync(assignmentId);
        if (assignment == null)
        {
            return NotFound($"Assignment with ID {assignmentId} not found");
        }

        _context.TaskAssignments.Remove(assignment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateTeamRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(1, ErrorMessage = "Name cannot be empty")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "ProjectId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "ProjectId must be greater than 0")]
    public int ProjectId { get; set; }
}

public class UpdateTeamRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(1, ErrorMessage = "Name cannot be empty")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
}

public class AddMemberRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(1, ErrorMessage = "Name cannot be empty")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }
    
    [MaxLength(50, ErrorMessage = "GitUsername cannot exceed 50 characters")]
    public string? GitUsername { get; set; }
}

public class UpdateMemberRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(1, ErrorMessage = "Name cannot be empty")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }
    
    [MaxLength(50, ErrorMessage = "GitUsername cannot exceed 50 characters")]
    public string? GitUsername { get; set; }
}

public class AssignTaskRequest
{
    [Required(ErrorMessage = "TaskId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "TaskId must be greater than 0")]
    public int TaskId { get; set; }
    
    [Required(ErrorMessage = "TeamMemberId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "TeamMemberId must be greater than 0")]
    public int TeamMemberId { get; set; }
    
    [Required(ErrorMessage = "Role is required")]
    public TaskRole Role { get; set; }
}

