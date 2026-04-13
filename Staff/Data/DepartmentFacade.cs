using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public class DepartmentFacade(ApplicationDbContext dbContext)
{
    public async Task<Department> CreateAsync(Department department, CancellationToken cancellationToken = default)
    {
        dbContext.Departments.Add(department);
        await dbContext.SaveChangesAsync(cancellationToken);

        return department;
    }

    public async Task<Department> SaveAsync(Department department, CancellationToken cancellationToken = default)
    {
        dbContext.Departments.Update(department);
        await dbContext.SaveChangesAsync(cancellationToken);

        return department;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await dbContext.Departments.FindAsync([id], cancellationToken);
        if (department is null)
        {
            return;
        }

        dbContext.Departments.Remove(department);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Department>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        return await dbContext.Departments
            .AsNoTracking()
            .Include(x => x.Organization)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
