using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public class EmployeeFacade(ApplicationDbContext dbContext)
{
    public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Employees
            .AsNoTracking()
            .Include(x => x.Organization)
            .Include(x => x.Position)
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Employee>> GetByOrganizationIdAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Employees
            .AsNoTracking()
            .Include(x => x.Organization)
            .Include(x => x.Position)
            .Include(x => x.User)
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.MiddleName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return employee;
    }

    public async Task<Employee> SaveAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        dbContext.Employees.Update(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return employee;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FindAsync([id], cancellationToken);
        if (employee is null)
        {
            return;
        }

        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        return await dbContext.Employees
            .AsNoTracking()
            .Include(x => x.Organization)
            .Include(x => x.Position)
            .Include(x => x.User)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.MiddleName)
            .ToListAsync(cancellationToken);
    }
}
