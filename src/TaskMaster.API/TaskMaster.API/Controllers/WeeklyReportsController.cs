using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMaster.API.Extensions;
using TaskMaster.API.Models;
using TaskMaster.Core.Data;
using TaskMaster.Core.Models;
using TaskMaster.Core.Services;

namespace TaskMaster.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeeklyReportsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWeeklyReportService _reportService;

    public WeeklyReportsController(AppDbContext context, IWeeklyReportService reportService)
    {
        _context = context;
        _reportService = reportService;
    }

    /// <summary>
    /// Gets all weekly reports, optionally filtered by project with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<WeeklyReport>>> GetWeeklyReports(
        [FromQuery] int? projectId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        // Validate pagination parameters
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.WeeklyReports
            .Include(r => r.Project)
            .AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(r => r.ProjectId == projectId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(r => r.WeekStartDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.WeekEndDate <= endDate.Value);
        }

        query = query.OrderByDescending(r => r.WeekStartDate);

        var pagedQuery = new PagedQuery { PageNumber = pageNumber, PageSize = pageSize };
        var result = await query.ToPagedResultAsync(pagedQuery);

        return Ok(result);
    }

    /// <summary>
    /// Gets a specific weekly report by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WeeklyReport>> GetWeeklyReport(int id)
    {
        var report = await _reportService.GetWeeklyReportByIdAsync(id);
        
        if (report == null)
        {
            return NotFound();
        }

        return Ok(report);
    }

    /// <summary>
    /// Generates a new weekly report for a project
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<WeeklyReport>> GenerateWeeklyReport([FromBody] GenerateReportRequest request)
    {
        // Validate project exists
        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project == null)
        {
            return NotFound($"Project with ID {request.ProjectId} not found");
        }

        // Default to current week if not specified
        var weekStart = request.WeekStart ?? GetStartOfWeek(DateTime.UtcNow);
        var weekEnd = request.WeekEnd ?? weekStart.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);

        if (weekEnd <= weekStart)
        {
            return BadRequest("Week end date must be after week start date");
        }

        try
        {
            var report = await _reportService.GenerateWeeklyReportAsync(
                request.ProjectId,
                weekStart,
                weekEnd,
                request.TemplateContent);

            return CreatedAtAction(
                nameof(GetWeeklyReport),
                new { id = report.Id },
                report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to generate report", message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a weekly report
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWeeklyReport(int id)
    {
        var report = await _context.WeeklyReports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        await _reportService.DeleteWeeklyReportAsync(id);
        return NoContent();
    }

    private DateTime GetStartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
}

public class GenerateReportRequest
{
    [Required(ErrorMessage = "ProjectId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "ProjectId must be greater than 0")]
    public int ProjectId { get; set; }
    
    public DateTime? WeekStart { get; set; }
    
    public DateTime? WeekEnd { get; set; }
    
    public string? TemplateContent { get; set; }
}

