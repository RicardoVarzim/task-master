namespace TaskMaster.Core.Models;

/// <summary>
/// Enumeration representing roles that can be assigned to tasks
/// </summary>
public enum TaskRole
{
    /// <summary>
    /// Person who requested the task
    /// </summary>
    Requester = 0,

    /// <summary>
    /// Person analyzing requirements
    /// </summary>
    Analyst = 1,

    /// <summary>
    /// Person developing the task
    /// </summary>
    Developer = 2,

    /// <summary>
    /// Person reviewing the work
    /// </summary>
    Reviewer = 3,

    /// <summary>
    /// Person testing the implementation
    /// </summary>
    Tester = 4,

    /// <summary>
    /// Person managing the task
    /// </summary>
    Manager = 5,

    /// <summary>
    /// Other role
    /// </summary>
    Other = 6
}

