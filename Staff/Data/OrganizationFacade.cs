using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public class OrganizationFacade(ApplicationDbContext dbContext)
{
    /// <summary>
    /// Возвращает организацию по id вместе с её сотрудниками и их должностями.
    /// </summary>
    /// <param name="id">Id организации, которую нужно найти.</param>
    /// <param name="cancellationToken">Токен для отмены запроса.</param>
    /// <returns>Найденная организация или <see langword="null" />, если запись не найдена.</returns>
    public async Task<Organization?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Organizations
            .AsNoTracking()
            // Сразу грузим сотрудников с должностями, чтобы карточка собралась одним запросом.
            .Include(x => x.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ThenBy(e => e.MiddleName))
            .ThenInclude(x => x.Position)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// Создаёт новую организацию в базе.
    /// </summary>
    /// <param name="organization">Организация, которую нужно сохранить.</param>
    /// <param name="cancellationToken">Токен для отмены сохранения.</param>
    /// <returns>Созданная организация.</returns>
    public async Task<Organization> CreateAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        dbContext.Organizations.Add(organization);
        await dbContext.SaveChangesAsync(cancellationToken);

        return organization;
    }

    /// <summary>
    /// Сохраняет изменения у уже существующей организации.
    /// </summary>
    /// <param name="organization">Организация с новыми данными.</param>
    /// <param name="cancellationToken">Токен для отмены сохранения.</param>
    /// <returns>Та же организация после сохранения.</returns>
    public async Task<Organization> SaveAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        dbContext.Organizations.Update(organization);
        await dbContext.SaveChangesAsync(cancellationToken);

        return organization;
    }

    /// <summary>
    /// Удаляет организацию, если в ней не осталось сотрудников.
    /// </summary>
    /// <param name="id">Id организации для удаления.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача без значения результата.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если в организации ещё есть сотрудники.</exception>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var organization = await dbContext.Organizations
            .Include(x => x.Employees)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (organization is null)
        {
            return;
        }

        if (organization.Employees.Count > 0)
        {
            // Пока внутри есть сотрудники, удаление лучше не разрешать.
            throw new InvalidOperationException("Нельзя удалить организацию, пока в ней есть сотрудники.");
        }

        dbContext.Organizations.Remove(organization);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает список всех организаций вместе с количеством связанных сотрудников.
    /// </summary>
    /// <param name="cancellationToken">Токен для отмены загрузки списка.</param>
    /// <returns>Список организаций, отсортированный по названию.</returns>
    public async Task<List<Organization>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Задержка тут оставлена специально, чтобы на UI было видно состояние загрузки.
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        return await dbContext.Organizations
            .AsNoTracking()
            .Include(x => x.Employees)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
