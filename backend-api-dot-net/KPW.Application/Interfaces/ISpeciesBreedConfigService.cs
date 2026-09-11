using KPW.Application.DTOs.Species;

namespace KPW.Application.Interfaces;

public interface ISpeciesBreedConfigService
{
    Task<SpeciesBreedConfigDto> GetConfigAsync(CancellationToken cancellationToken = default);
    Task<SpeciesBreedConfigDto> UpdateConfigAsync(List<SpeciesConfigDto> species, string modifiedBy, CancellationToken cancellationToken = default);
    Task<SpeciesBreedConfigDto> ResetToDefaultsAsync(string modifiedBy, CancellationToken cancellationToken = default);
}
