using Microsoft.AspNetCore.Identity;

namespace Staff.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public Employee? Employee { get; set; }
}

