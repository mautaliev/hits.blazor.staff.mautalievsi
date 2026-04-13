using Microsoft.AspNetCore.Identity;

namespace Staff.Data;

public class ApplicationUserRole : IdentityUserRole<string>
{
    public ApplicationUser User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
