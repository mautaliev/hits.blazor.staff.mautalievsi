using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public class EmployeeManagementService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager)
{
    private const string GeneratedPassword = "Temp123!";

    public async Task<Employee> CreateAsync(EmployeeUpsertModel model, CancellationToken cancellationToken = default)
    {
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
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(employee.User);
            var resetResult = await userManager.ResetPasswordAsync(employee.User, resetToken, model.Password);
            EnsureSuccess(resetResult);
        }

        var currentRoles = await userManager.GetRolesAsync(employee.User);
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

        var deleteUserResult = await userManager.DeleteAsync(employee.User);
        EnsureSuccess(deleteUserResult);
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(x => x.Description));
        throw new InvalidOperationException(errors);
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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

        return $"employee-{Guid.NewGuid():N}@local";
    }

    private static string ResolvePassword(EmployeeUpsertModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            return model.Password;
        }

        return GeneratedPassword;
    }

    private static int? NormalizePositionId(int? positionId)
    {
        if (!positionId.HasValue || positionId.Value <= 0)
        {
            return null;
        }

        return positionId.Value;
    }
}
