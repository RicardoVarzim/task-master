using Microsoft.EntityFrameworkCore;
using TaskMaster.Core.Models;
using TaskModel = TaskMaster.Core.Models.Task;

namespace TaskMaster.Core.Data;

/// <summary>
/// Database context for Task Master application
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskModel> Tasks { get; set; }
    public DbSet<TaskTag> TaskTags { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<TaskAssignment> TaskAssignments { get; set; }
    public DbSet<GitCommit> GitCommits { get; set; }
    public DbSet<TaskChangeHistory> TaskChangeHistories { get; set; }
    public DbSet<WeeklyReport> WeeklyReports { get; set; }
    public DbSet<CheckInHistory> CheckInHistories { get; set; }
    public DbSet<TaskMetrics> TaskMetrics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FullPath).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FullPath).IsRequired().HasMaxLength(1000);
        });

        // Task configuration
        modelBuilder.Entity<TaskModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SourceFilePath, e.LineNumber });
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.SourceFilePath).IsRequired().HasMaxLength(1000);
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskTag configuration (many-to-many with Task)
        modelBuilder.Entity<TaskTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // Many-to-many relationship between Task and TaskTag
        modelBuilder.Entity<TaskModel>()
            .HasMany(t => t.Tags)
            .WithMany(tag => tag.Tasks)
            .UsingEntity<Dictionary<string, object>>(
                "TaskTaskTag",
                j => j.HasOne<TaskTag>().WithMany().HasForeignKey("TaskTagId"),
                j => j.HasOne<TaskModel>().WithMany().HasForeignKey("TaskId")
            );

        // Team configuration
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Teams)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TeamMember configuration
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.GitUsername).HasMaxLength(100);
            entity.HasOne(e => e.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskAssignment configuration
        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TaskId, e.TeamMemberId, e.Role });
            entity.HasOne(e => e.Task)
                .WithMany(t => t.Assignments)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.TeamMember)
                .WithMany(m => m.TaskAssignments)
                .HasForeignKey(e => e.TeamMemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GitCommit configuration
        modelBuilder.Entity<GitCommit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProjectId, e.CommitHash });
            entity.Property(e => e.CommitHash).IsRequired().HasMaxLength(40);
            entity.Property(e => e.AuthorName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AuthorEmail).HasMaxLength(255);
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskChangeHistory configuration
        modelBuilder.Entity<TaskChangeHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TaskId, e.ChangedAt });
            entity.Property(e => e.ChangeType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AuthorName).HasMaxLength(200);
            entity.Property(e => e.AuthorEmail).HasMaxLength(255);
            entity.HasOne(e => e.Task)
                .WithMany(t => t.ChangeHistory)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.GitCommit)
                .WithMany(c => c.ChangeHistory)
                .HasForeignKey(e => e.GitCommitId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // WeeklyReport configuration
        modelBuilder.Entity<WeeklyReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProjectId, e.WeekIdentifier });
            entity.Property(e => e.WeekIdentifier).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CheckInHistory configuration
        modelBuilder.Entity<CheckInHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProjectId, e.CheckInDate });
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskMetrics configuration
        modelBuilder.Entity<TaskMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProjectId, e.PeriodStart, e.PeriodEnd });
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

