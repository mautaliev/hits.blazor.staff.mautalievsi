using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Staff.Data;

public static class AppDemoDataSeeder
{
    private const string DefaultUserPassword = "Demo123!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedOrganizationsAsync(dbContext);
        await SeedPositionsAsync(dbContext);
        await SeedEmployeesAsync(dbContext, userManager);
    }

    private static async Task SeedOrganizationsAsync(ApplicationDbContext dbContext)
    {
        var organizations = new[]
        {
            new Organization
            {
                Name = "ООО \"Рога и копыта\"",
                Description = "Классическая демонстрационная организация для кадрового учёта.",
                Inn = "7701234567",
                Kpp = "770101001"
            },
            new Organization
            {
                Name = "ООО \"Ромашка\"",
                Description = "Компания для демонстрации независимого кадрового состава.",
                Inn = "7802345678",
                Kpp = "780201001"
            },
            new Organization
            {
                Name = "ИП \"Мауталиев С. И.\"",
                Description = "Индивидуальный предприниматель с небольшим штатом.",
                Inn = "667345678901",
                Kpp = null
            }
        };

        foreach (var organization in organizations)
        {
            if (await dbContext.Organizations.AnyAsync(x => x.Name == organization.Name))
            {
                continue;
            }

            dbContext.Organizations.Add(organization);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedPositionsAsync(ApplicationDbContext dbContext)
    {
        var positions = new[]
        {
            new Position
            {
                Name = "Кадровик",
                Description = "Ведёт кадровые документы и сопровождает сотрудников.",
                Salary = 75000m
            },
            new Position
            {
                Name = "Юрист",
                Description = "Сопровождает организацию по юридическим вопросам.",
                Salary = 90000m
            },
            new Position
            {
                Name = "Директор",
                Description = "Руководит организацией и отвечает за стратегические решения.",
                Salary = 120000m
            },
            new Position
            {
                Name = "Бухгалтер",
                Description = "Ведёт расчёты и бухгалтерский документооборот.",
                Salary = 80000m
            },
            new Position
            {
                Name = "Менеджер",
                Description = "Координирует текущие процессы и взаимодействие с клиентами.",
                Salary = 70000m
            }
        };

        foreach (var position in positions)
        {
            if (await dbContext.Positions.AnyAsync(x => x.Name == position.Name))
            {
                continue;
            }

            dbContext.Positions.Add(position);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedEmployeesAsync(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        var organizations = await dbContext.Organizations.ToDictionaryAsync(x => x.Name);
        var positions = await dbContext.Positions.ToDictionaryAsync(x => x.Name);

        var employees = new[]
        {
            new DemoEmployee(
                "Петрова",
                "Анна",
                "Игоревна",
                "hr.roga@example.com",
                "770000000001",
                "11200000001",
                "4001",
                "123456",
                "ГУ МВД России по г. Москве",
                new DateOnly(2018, 5, 14),
                "770-001",
                "ООО \"Рога и копыта\"",
                "Кадровик",
                AppRoles.Hr),
            new DemoEmployee(
                "Сидоров",
                "Максим",
                "Олегович",
                "lawyer.roga@example.com",
                "770000000002",
                "11200000002",
                "4002",
                "234567",
                "ГУ МВД России по г. Москве",
                new DateOnly(2017, 9, 21),
                "770-002",
                "ООО \"Рога и копыта\"",
                "Юрист",
                AppRoles.Lawyer),
            new DemoEmployee(
                "Иванов",
                "Сергей",
                "Павлович",
                "director.roga@example.com",
                "770000000003",
                "11200000003",
                "4003",
                "345678",
                "ГУ МВД России по г. Москве",
                new DateOnly(2016, 3, 11),
                "770-003",
                "ООО \"Рога и копыта\"",
                "Директор",
                null),
            new DemoEmployee(
                "Орлова",
                "Елена",
                "Викторовна",
                "director.romashka@example.com",
                "780000000001",
                "11300000001",
                "5001",
                "456789",
                "ГУ МВД России по Санкт-Петербургу",
                new DateOnly(2015, 7, 19),
                "780-001",
                "ООО \"Ромашка\"",
                "Директор",
                null),
            new DemoEmployee(
                "Морозов",
                "Денис",
                "Андреевич",
                "accountant.romashka@example.com",
                "780000000002",
                "11300000002",
                "5002",
                "567890",
                "ГУ МВД России по Санкт-Петербургу",
                new DateOnly(2019, 1, 28),
                "780-002",
                "ООО \"Ромашка\"",
                "Бухгалтер",
                null),
            new DemoEmployee(
                "Соколова",
                "Мария",
                "Николаевна",
                "manager.romashka@example.com",
                "780000000003",
                "11300000003",
                "5003",
                "678901",
                "ГУ МВД России по Санкт-Петербургу",
                new DateOnly(2020, 10, 4),
                "780-003",
                "ООО \"Ромашка\"",
                "Менеджер",
                null),
            new DemoEmployee(
                "Мауталиев",
                "Саид",
                "Ильдарович",
                "owner.mautaliev@example.com",
                "667000000001",
                "11400000001",
                "6601",
                "789012",
                "ГУ МВД России по Свердловской области",
                new DateOnly(2014, 4, 9),
                "660-001",
                "ИП \"Мауталиев С. И.\"",
                "Директор",
                null),
            new DemoEmployee(
                "Фёдорова",
                "Наталья",
                "Сергеевна",
                "accountant.mautaliev@example.com",
                "667000000002",
                "11400000002",
                "6602",
                "890123",
                "ГУ МВД России по Свердловской области",
                new DateOnly(2018, 8, 30),
                "660-002",
                "ИП \"Мауталиев С. И.\"",
                "Бухгалтер",
                null),
            new DemoEmployee(
                "Кузнецов",
                "Артём",
                "Владимирович",
                "manager.mautaliev@example.com",
                "667000000003",
                "11400000003",
                "6603",
                "901234",
                "ГУ МВД России по Свердловской области",
                new DateOnly(2021, 2, 15),
                "660-003",
                "ИП \"Мауталиев С. И.\"",
                "Менеджер",
                null)
        };

        foreach (var item in employees)
        {
            if (await dbContext.Employees.Include(x => x.User).AnyAsync(x => x.User.Email == item.Email))
            {
                continue;
            }

            var user = new ApplicationUser
            {
                UserName = item.Email,
                Email = item.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, DefaultUserPassword);
            EnsureSuccess(createResult, item.Email);

            if (!string.IsNullOrWhiteSpace(item.RoleName))
            {
                var roleResult = await userManager.AddToRoleAsync(user, item.RoleName);
                EnsureSuccess(roleResult, item.Email);
            }

            dbContext.Employees.Add(new Employee
            {
                LastName = item.LastName,
                FirstName = item.FirstName,
                MiddleName = item.MiddleName,
                Inn = item.Inn,
                Snils = item.Snils,
                PassportSeries = item.PassportSeries,
                PassportNumber = item.PassportNumber,
                PassportIssuedBy = item.PassportIssuedBy,
                PassportIssuedDate = item.PassportIssuedDate,
                PassportDepartmentCode = item.PassportDepartmentCode,
                OrganizationId = organizations[item.OrganizationName].Id,
                PositionId = positions[item.PositionName].Id,
                UserId = user.Id
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static void EnsureSuccess(IdentityResult result, string email)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(x => x.Description));
        throw new InvalidOperationException($"Не удалось создать пользователя {email}: {errors}");
    }

    private sealed record DemoEmployee(
        string LastName,
        string FirstName,
        string MiddleName,
        string Email,
        string Inn,
        string Snils,
        string PassportSeries,
        string PassportNumber,
        string PassportIssuedBy,
        DateOnly PassportIssuedDate,
        string PassportDepartmentCode,
        string OrganizationName,
        string PositionName,
        string? RoleName);
}
