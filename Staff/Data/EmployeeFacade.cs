using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public class EmployeeFacade(ApplicationDbContext dbContext)
{
    /// <summary>
    /// Возвращает сотрудника по id вместе с организацией, должностью и ролями пользователя.
    /// </summary>
    /// <param name="id">Id сотрудника.</param>
    /// <param name="cancellationToken">Токен для отмены запроса.</param>
    /// <returns>Найденный сотрудник или <see langword="null" />, если его нет в базе.</returns>
    public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Employees
            .AsNoTracking()
            // Для детальной карточки подтягиваем всё, что там потом показывается.
            .Include(x => x.Organization)
            .Include(x => x.Position)
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// Возвращает всех сотрудников конкретной организации.
    /// </summary>
    /// <param name="organizationId">Id организации, чьих сотрудников нужно получить.</param>
    /// <param name="cancellationToken">Токен для отмены запроса.</param>
    /// <returns>Список сотрудников выбранной организации.</returns>
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

    /// <summary>
    /// Создаёт нового сотрудника в базе.
    /// </summary>
    /// <param name="employee">Сотрудник, которого нужно сохранить.</param>
    /// <param name="cancellationToken">Токен для отмены сохранения.</param>
    /// <returns>Созданный сотрудник.</returns>
    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return employee;
    }

    /// <summary>
    /// Сохраняет изменения у существующего сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник с обновлёнными данными.</param>
    /// <param name="cancellationToken">Токен для отмены сохранения.</param>
    /// <returns>Тот же сотрудник после сохранения.</returns>
    public async Task<Employee> SaveAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        dbContext.Employees.Update(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return employee;
    }

    /// <summary>
    /// Удаляет сотрудника по id, если такая запись существует.
    /// </summary>
    /// <param name="id">Id сотрудника для удаления.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача без значения результата.</returns>
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

    /// <summary>
    /// Возвращает список всех сотрудников вместе с основной связанной информацией.
    /// </summary>
    /// <param name="cancellationToken">Токен для отмены загрузки списка.</param>
    /// <returns>Список сотрудников, отсортированный по ФИО.</returns>
    public async Task<List<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Здесь тоже задержка чисто демонстрационная, для "Загрузка...".
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
