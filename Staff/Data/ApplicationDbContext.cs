using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Position> Positions => Set<Position>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Organization>(entity =>
        {
            entity.HasIndex(x => x.Inn).IsUnique();
        });

        builder.Entity<Department>(entity =>
        {
            entity.HasOne(x => x.Organization)
                .WithMany(x => x.Departments)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
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

            entity.HasOne(x => x.Department)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.DepartmentId)
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

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "role-admin",
                ConcurrencyStamp = "role-admin-stamp",
                Name = "Админ",
                NormalizedName = "АДМИН"
            },
            new IdentityRole
            {
                Id = "role-hr",
                ConcurrencyStamp = "role-hr-stamp",
                Name = "Кадровик",
                NormalizedName = "КАДРОВИК"
            },
            new IdentityRole
            {
                Id = "role-lawyer",
                ConcurrencyStamp = "role-lawyer-stamp",
                Name = "Юрист",
                NormalizedName = "ЮРИСТ"
            });
    }
}
