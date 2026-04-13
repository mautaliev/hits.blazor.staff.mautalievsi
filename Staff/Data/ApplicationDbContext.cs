using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<
        ApplicationUser,
        Role,
        string,
        IdentityUserClaim<string>,
        ApplicationUserRole,
        IdentityUserLogin<string>,
        IdentityRoleClaim<string>,
        IdentityUserToken<string>>(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Position> Positions => Set<Position>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Organization>(entity =>
        {
            entity.HasIndex(x => x.Inn).IsUnique();
        });

        builder.Entity<Position>(entity =>
        {
            entity.Property(x => x.Salary).HasPrecision(18, 2);
        });

        builder.Entity<Employee>(entity =>
        {
            entity.HasIndex(x => x.Inn).IsUnique();
            entity.HasIndex(x => x.Snils).IsUnique();
            entity.HasIndex(x => x.UserId).IsUnique();

            entity.HasOne(x => x.Organization)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Position)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithOne(x => x.Employee)
                .HasForeignKey<Employee>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ApplicationUserRole>(entity =>
        {
            entity.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Role>().HasData(
            new Role
            {
                Id = "role-admin",
                ConcurrencyStamp = "role-admin-stamp",
                Name = "Админ",
                NormalizedName = "АДМИН"
            },
            new Role
            {
                Id = "role-hr",
                ConcurrencyStamp = "role-hr-stamp",
                Name = "Кадровик",
                NormalizedName = "КАДРОВИК"
            },
            new Role
            {
                Id = "role-lawyer",
                ConcurrencyStamp = "role-lawyer-stamp",
                Name = "Юрист",
                NormalizedName = "ЮРИСТ"
            });
    }
}
