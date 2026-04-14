namespace Staff.Data;

public static class AppRoles
{
    public const string Admin = "Админ";
    public const string Hr = "Кадровик";
    public const string Lawyer = "Юрист";

    public const string ManagementAccess = $"{Admin},{Hr},{Lawyer}";
    public const string OrganizationEditors = $"{Admin},{Lawyer}";
    public const string PositionEditors = $"{Admin},{Hr}";
    public const string EmployeeEditors = $"{Admin},{Hr}";
}
