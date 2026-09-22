namespace Asisya.Domain.Entities;

/// <summary>
/// Represents an internal staff member or sales representative within ASISYA.
/// Supports hierarchical organizational reporting structures.
/// </summary>
public class Employee
{
    /// <summary>
    /// Unique numeric identifier for the Employee (Primary Key).
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Legal family/last name of the employee.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Legal given/first name of the employee.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Job title or corporate role assignment.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Courtesy title used in communications (e.g., Ms., Mr., Dr.).
    /// </summary>
    public string? TitleOfCourtesy { get; set; }

    /// <summary>
    /// Official date of birth.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Date when employment started with ASISYA.
    /// </summary>
    public DateTime? HireDate { get; set; }

    /// <summary>
    /// Residential street address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Residential city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Residential state, department, or region.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Postal or ZIP routing code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Residential country.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Personal contact telephone.
    /// </summary>
    public string? HomePhone { get; set; }

    /// <summary>
    /// Internal PBX telephone extension.
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// Digital photo of the employee.
    /// </summary>
    public byte[]? Photo { get; set; }

    /// <summary>
    /// Administrative or performance notes regarding employee background.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Foreign key referencing the manager or superior employee (Self-referencing relationship).
    /// </summary>
    public int? ReportsTo { get; set; }

    /// <summary>
    /// Navigation property to the employee's direct organizational manager.
    /// </summary>
    public Employee? Manager { get; set; }

    /// <summary>
    /// Navigation collection of employees directly reporting to this employee.
    /// </summary>
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    /// <summary>
    /// Navigation collection of orders managed or fulfilled by this employee.
    /// </summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
