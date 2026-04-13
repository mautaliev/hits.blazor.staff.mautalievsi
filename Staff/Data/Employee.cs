using System.ComponentModel.DataAnnotations;

namespace Staff.Data;

public class Employee
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string MiddleName { get; set; } = string.Empty;

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

    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public int PositionId { get; set; }

    public Position Position { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;
}
