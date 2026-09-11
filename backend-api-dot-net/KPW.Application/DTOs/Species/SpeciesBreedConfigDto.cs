namespace KPW.Application.DTOs.Species;

public record BreedConfigDto(
    string Name,
    string? SizeCategory = null,
    string? Conformation = null,
    string? Notes = null
);

public record SpeciesConfigDto(
    string Name,
    string DisplayName,
    string? Icon = null,
    List<BreedConfigDto>? Breeds = null
);

public record SpeciesBreedConfigDto(
    List<SpeciesConfigDto> Species,
    DateTimeOffset? LastModifiedAt = null,
    string? LastModifiedBy = null
);

public record UpdateSpeciesBreedConfigRequestDto(
    List<SpeciesConfigDto> Species
);
