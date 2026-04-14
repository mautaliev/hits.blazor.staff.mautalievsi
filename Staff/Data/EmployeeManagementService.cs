using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public class EmployeeManagementService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager)
{
    private const string GeneratedPassword = "Temp123!";

    /// <summary>
    /// Создаёт сотрудника и связанного с ним пользователя Identity.
    /// </summary>
    /// <param name="model">Данные из формы создания сотрудника.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Созданный сотрудник.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если не удалось создать пользователя или назначить роль.</exception>
    public async Task<Employee> CreateAsync(EmployeeUpsertModel model, CancellationToken cancellationToken = default)
    {
        // Эти значения подготавливаем сразу, чтобы дальше метод был чуть чище.
        var login = ResolveLogin(model);
        var password = ResolvePassword(model);

        var user = new ApplicationUser
        {
            UserName = login,
            Email = login,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        EnsureSuccess(createResult);

        // Если роль не выбрали, пользователь просто создаётся без неё.
        if (!string.IsNullOrWhiteSpace(model.RoleName))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, model.RoleName);
            EnsureSuccess(addRoleResult);
        }

        var employee = new Employee
        {
            LastName = model.LastName,
            FirstName = model.FirstName,
            MiddleName = model.MiddleName,
            Inn = NullIfEmpty(model.Inn),
            Snils = NullIfEmpty(model.Snils),
            PassportSeries = NullIfEmpty(model.PassportSeries),
            PassportNumber = NullIfEmpty(model.PassportNumber),
            PassportIssuedBy = NullIfEmpty(model.PassportIssuedBy),
            PassportIssuedDate = model.PassportIssuedDate,
            PassportDepartmentCode = NullIfEmpty(model.PassportDepartmentCode),
            OrganizationId = model.OrganizationId,
            PositionId = NormalizePositionId(model.PositionId),
            UserId = user.Id
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return employee;
    }

    /// <summary>
    /// Обновляет данные сотрудника, его логин, пароль и управляемую роль.
    /// </summary>
    /// <param name="employeeId">Id сотрудника, которого нужно обновить.</param>
    /// <param name="model">Новые данные из формы редактирования.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Обновлённый сотрудник.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если сотрудник не найден или операция Identity завершилась с ошибкой.</exception>
    public async Task<Employee> UpdateAsync(int employeeId, EmployeeUpsertModel model, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken)
            ?? throw new InvalidOperationException("Сотрудник не найден.");

        employee.LastName = model.LastName;
        employee.FirstName = model.FirstName;
        employee.MiddleName = model.MiddleName;
        employee.Inn = NullIfEmpty(model.Inn);
        employee.Snils = NullIfEmpty(model.Snils);
        employee.PassportSeries = NullIfEmpty(model.PassportSeries);
        employee.PassportNumber = NullIfEmpty(model.PassportNumber);
        employee.PassportIssuedBy = NullIfEmpty(model.PassportIssuedBy);
        employee.PassportIssuedDate = model.PassportIssuedDate;
        employee.PassportDepartmentCode = NullIfEmpty(model.PassportDepartmentCode);
        employee.OrganizationId = model.OrganizationId;
        employee.PositionId = NormalizePositionId(model.PositionId);

        var login = ResolveLogin(model, employee.User.Email ?? employee.User.UserName ?? string.Empty);

        // Логин и email тут специально держим одинаковыми, чтобы не путаться.
        if (!string.Equals(employee.User.UserName, login, StringComparison.OrdinalIgnoreCase))
        {
            var userNameResult = await userManager.SetUserNameAsync(employee.User, login);
            EnsureSuccess(userNameResult);
        }

        if (!string.Equals(employee.User.Email, login, StringComparison.OrdinalIgnoreCase))
        {
            var emailResult = await userManager.SetEmailAsync(employee.User, login);
            EnsureSuccess(emailResult);
            employee.User.EmailConfirmed = true;
        }

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            // Пароль меняем через reset token, так безопаснее и это штатный путь для Identity.
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(employee.User);
            var resetResult = await userManager.ResetPasswordAsync(employee.User, resetToken, model.Password);
            EnsureSuccess(resetResult);
        }

        var currentRoles = await userManager.GetRolesAsync(employee.User);
        // Убираем только "наши" управляемые роли, остальные роли приложения не трогаем.
        var managedRoles = currentRoles.Where(x => x is AppRoles.Admin or AppRoles.Hr or AppRoles.Lawyer).ToArray();
        if (managedRoles.Length > 0)
        {
            var removeRolesResult = await userManager.RemoveFromRolesAsync(employee.User, managedRoles);
            EnsureSuccess(removeRolesResult);
        }

        if (!string.IsNullOrWhiteSpace(model.RoleName))
        {
            var addRoleResult = await userManager.AddToRoleAsync(employee.User, model.RoleName);
            EnsureSuccess(addRoleResult);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return employee;
    }

    /// <summary>
    /// Удаляет сотрудника и связанный с ним аккаунт пользователя.
    /// </summary>
    /// <param name="employeeId">Id сотрудника для удаления.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача без значения результата.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если не удалось удалить связанного пользователя.</exception>
    public async Task DeleteAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);

        if (employee is null)
        {
            return;
        }

        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Сначала чистим кадровую запись, потом уже сам аккаунт пользователя.
        var deleteUserResult = await userManager.DeleteAsync(employee.User);
        EnsureSuccess(deleteUserResult);
    }

    /// <summary>
    /// Проверяет результат операции Identity и выбрасывает исключение, если что-то пошло не так.
    /// </summary>
    /// <param name="result">Результат операции Identity.</param>
    /// <exception cref="InvalidOperationException">Выбрасывается, если операция завершилась с ошибками.</exception>
    private static void EnsureSuccess(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(x => x.Description));
        throw new InvalidOperationException(errors);
    }

    /// <summary>
    /// Убирает пустые строки и возвращает <see langword="null" />, если значения по сути нет.
    /// </summary>
    /// <param name="value">Исходная строка из формы или модели.</param>
    /// <returns>Обрезанная строка либо <see langword="null" />.</returns>
    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// Определяет логин сотрудника: сначала из формы, потом из старого значения, а если ничего нет, генерирует служебный.
    /// </summary>
    /// <param name="model">Модель с введёнными пользователем данными.</param>
    /// <param name="fallbackLogin">Запасной логин, который можно использовать при обновлении.</param>
    /// <returns>Готовый логин для пользователя.</returns>
    private static string ResolveLogin(EmployeeUpsertModel model, string? fallbackLogin = null)
    {
        if (!string.IsNullOrWhiteSpace(model.Login))
        {
            return model.Login.Trim();
        }

        if (!string.IsNullOrWhiteSpace(fallbackLogin))
        {
            return fallbackLogin;
        }

        // Если логина нет вообще, генерируем служебный.
        return $"employee-{Guid.NewGuid():N}@local";
    }

    /// <summary>
    /// Возвращает пароль из формы или подставляет временный пароль по умолчанию.
    /// </summary>
    /// <param name="model">Модель с данными сотрудника.</param>
    /// <returns>Пароль, который пойдёт в Identity.</returns>
    private static string ResolvePassword(EmployeeUpsertModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            return model.Password;
        }

        return GeneratedPassword;
    }

    /// <summary>
    /// Приводит id должности к нормальному виду: если выбора нет, возвращает <see langword="null" />.
    /// </summary>
    /// <param name="positionId">Id должности из формы.</param>
    /// <returns>Корректный id должности или <see langword="null" />.</returns>
    private static int? NormalizePositionId(int? positionId)
    {
        // Ноль из формы считаем как "должность не выбрана".
        if (!positionId.HasValue || positionId.Value <= 0)
        {
            return null;
        }

        return positionId.Value;
    }
}
