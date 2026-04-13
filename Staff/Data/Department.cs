using System.ComponentModel.DataAnnotations;

namespace Staff.Data;

public class Department
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public ICollection<Employee> Employees { get; set; } = [];
}
