namespace KPW.Domain.Enums;

public static class UserRole
{
    public const string SysAdmin = "SysAdmin";
    public const string Physio = "Physio";
    public const string Owner = "Owner";

    public static readonly string[] All = [SysAdmin, Physio, Owner];
}
