using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Staff.Data;

public class Role : IdentityRole
{
    [Required]
    [MaxLength(256)]
    public override string? Name { get; set; }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
}
