namespace KPW.Domain.Enums;

public static class PetSpecies
{
    public const string Canine = "Canine";
    public const string Feline = "Feline";
    public const string Equine = "Equine";
    public const string Avian = "Avian";
    public const string Other = "Other";

    public static readonly string[] All = [Canine, Feline, Equine, Avian, Other];
}
