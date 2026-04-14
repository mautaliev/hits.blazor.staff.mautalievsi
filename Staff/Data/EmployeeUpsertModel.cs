using System.ComponentModel.DataAnnotations;

namespace Staff.Data;

public class EmployeeUpsertModel
{
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string MiddleName { get; set; } = string.Empty;

    [EmailAddress]
    [Required]
    public string Login { get; set; } = string.Empty;

    public string? Password { get; set; }

    [MaxLength(12)]
    public string? Inn { get; set; }

    [MaxLength(11)]
    public string? Snils { get; set; }

    [MaxLength(4)]
    public string? PassportSeries { get; set; }

    [MaxLength(6)]
    public string? PassportNumber { get; set; }

    [MaxLength(255)]
    public string? PassportIssuedBy { get; set; }

    public DateOnly? PassportIssuedDate { get; set; }

    [MaxLength(20)]
    public string? PassportDepartmentCode { get; set; }

    [Range(1, int.MaxValue)]
    public int OrganizationId { get; set; }

    [Range(1, int.MaxValue)]
    public int PositionId { get; set; }

    public string? RoleName { get; set; }
}
