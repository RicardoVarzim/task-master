namespace TaskMaster.Core.Models;

/// <summary>
/// Enumeration representing task status
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is completed
    /// </summary>
    Completed = 0,

    /// <summary>
    /// Task is in progress
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Task is planned but not started
    /// </summary>
    Planned = 2,

    /// <summary>
    /// Task is blocked waiting for dependencies
    /// </summary>
    Blocked = 3,

    /// <summary>
    /// Task is pending action
    /// </summary>
    Pending = 4
}

