using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public class OrganizationFacade(ApplicationDbContext dbContext)
{
    public async Task<Organization?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Organizations
            .AsNoTracking()
            .Include(x => x.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ThenBy(e => e.MiddleName))
            .ThenInclude(x => x.Position)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Organization> CreateAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        dbContext.Organizations.Add(organization);
        await dbContext.SaveChangesAsync(cancellationToken);

        return organization;
    }

    public async Task<Organization> SaveAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        dbContext.Organizations.Update(organization);
        await dbContext.SaveChangesAsync(cancellationToken);

        return organization;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var organization = await dbContext.Organizations.FindAsync([id], cancellationToken);
        if (organization is null)
        {
            return;
        }

        dbContext.Organizations.Remove(organization);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Organization>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        return await dbContext.Organizations
            .AsNoTracking()
            .Include(x => x.Employees)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
