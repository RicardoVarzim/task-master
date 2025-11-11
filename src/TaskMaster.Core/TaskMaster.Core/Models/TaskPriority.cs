namespace TaskMaster.Core.Models;

/// <summary>
/// Enumeration representing task priority levels
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Maximum priority - Urgent, blocker, critical
    /// </summary>
    Maximum = 0,

    /// <summary>
    /// High priority - Important for platform management
    /// </summary>
    High = 1,

    /// <summary>
    /// Medium priority - Core development and improvements
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Low priority - Other integrations and minor bugs
    /// </summary>
    Low = 3,

    /// <summary>
    /// Strategic priority - Planning and analysis
    /// </summary>
    Strategic = 4,

    /// <summary>
    /// Maintenance priority - Code reviews and administrative tasks
    /// </summary>
    Maintenance = 5,

    /// <summary>
    /// Administrative priority - Personal/administrative tasks
    /// </summary>
    Administrative = 6
}

