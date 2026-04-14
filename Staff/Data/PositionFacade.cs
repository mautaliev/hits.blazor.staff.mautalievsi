using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public class PositionFacade(ApplicationDbContext dbContext)
{
    /// <summary>
    /// Возвращает должность по id вместе с сотрудниками, которые на неё назначены.
    /// </summary>
    /// <param name="id">Id должности, которую нужно найти.</param>
    /// <param name="cancellationToken">Токен для отмены запроса, если операция уже не нужна.</param>
    /// <returns>Найденная должность или <see langword="null" />, если такой записи нет.</returns>
    public async Task<Position?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Positions
            .AsNoTracking()
            // Для страницы должности сразу забираем сотрудников и их организации.
            .Include(x => x.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ThenBy(e => e.MiddleName))
            .ThenInclude(x => x.Organization)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// Создаёт новую должность в базе и возвращает её же после сохранения.
    /// </summary>
    /// <param name="position">Объект должности, который нужно добавить.</param>
    /// <param name="cancellationToken">Токен для отмены сохранения.</param>
    /// <returns>Созданная должность.</returns>
    public async Task<Position> CreateAsync(Position position, CancellationToken cancellationToken = default)
    {
        dbContext.Positions.Add(position);
        await dbContext.SaveChangesAsync(cancellationToken);

        return position;
    }

    /// <summary>
    /// Сохраняет изменения у уже существующей должности.
    /// </summary>
    /// <param name="position">Должность с обновлёнными данными.</param>
    /// <param name="cancellationToken">Токен для отмены сохранения.</param>
    /// <returns>Та же должность после сохранения.</returns>
    public async Task<Position> SaveAsync(Position position, CancellationToken cancellationToken = default)
    {
        dbContext.Positions.Update(position);
        await dbContext.SaveChangesAsync(cancellationToken);

        return position;
    }

    /// <summary>
    /// Удаляет должность по id, если на неё никто не назначен.
    /// </summary>
    /// <param name="id">Id должности, которую нужно удалить.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача без значения результата.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если у должности есть сотрудники.</exception>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var position = await dbContext.Positions
            .Include(x => x.Employees)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (position is null)
        {
            return;
        }

        if (position.Employees.Count > 0)
        {
            // Нельзя удалить должность, если на ней кто-то ещё висит.
            throw new InvalidOperationException("Нельзя удалить должность, пока на неё назначены сотрудники.");
        }

        dbContext.Positions.Remove(position);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает список всех должностей, отсортированный по названию.
    /// </summary>
    /// <param name="cancellationToken">Токен для отмены загрузки списка.</param>
    /// <returns>Список всех должностей из базы.</returns>
    public async Task<List<Position>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Та же учебная задержка для отображения лоадера.
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        return await dbContext.Positions
            .AsNoTracking()
            .Include(x => x.Employees)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
