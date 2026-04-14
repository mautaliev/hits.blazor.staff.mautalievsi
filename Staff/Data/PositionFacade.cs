using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public class PositionFacade(ApplicationDbContext dbContext)
{
    public async Task<Position?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Positions
            .AsNoTracking()
            .Include(x => x.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ThenBy(e => e.MiddleName))
            .ThenInclude(x => x.Organization)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Position> CreateAsync(Position position, CancellationToken cancellationToken = default)
    {
        dbContext.Positions.Add(position);
        await dbContext.SaveChangesAsync(cancellationToken);

        return position;
    }

    public async Task<Position> SaveAsync(Position position, CancellationToken cancellationToken = default)
    {
        dbContext.Positions.Update(position);
        await dbContext.SaveChangesAsync(cancellationToken);

        return position;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var position = await dbContext.Positions.FindAsync([id], cancellationToken);
        if (position is null)
        {
            return;
        }

        dbContext.Positions.Remove(position);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Position>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        return await dbContext.Positions
            .AsNoTracking()
            .Include(x => x.Employees)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
