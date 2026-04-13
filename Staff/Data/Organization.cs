using System.ComponentModel.DataAnnotations;

namespace Staff.Data;

public class Organization
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(12)]
    public string Inn { get; set; } = string.Empty;

    [MaxLength(9)]
    public string? Kpp { get; set; }

    public ICollection<Department> Departments { get; set; } = [];
}
