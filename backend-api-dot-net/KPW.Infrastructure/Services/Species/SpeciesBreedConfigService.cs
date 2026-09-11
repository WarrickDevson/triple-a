using System.Text.Json;
using KPW.Application.DTOs.Species;
using KPW.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KPW.Infrastructure.Services.Species;

public class SpeciesBreedConfigService : ISpeciesBreedConfigService
{
    private readonly string _filePath;
    private readonly ILogger<SpeciesBreedConfigService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private SpeciesBreedConfigDto? _cachedConfig;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public SpeciesBreedConfigService(IHostEnvironment environment, ILogger<SpeciesBreedConfigService> logger)
    {
        _logger = logger;
        var dir = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "species_breed_config.json");
    }

    public async Task<SpeciesBreedConfigDto> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedConfig != null)
        {
            return _cachedConfig;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedConfig != null)
            {
                return _cachedConfig;
            }

            if (File.Exists(_filePath))
            {
                var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
                var stored = JsonSerializer.Deserialize<StoredSpeciesBreedConfig>(json, JsonOptions);
                if (stored != null && stored.Species != null && stored.Species.Count > 0)
                {
                    _cachedConfig = new SpeciesBreedConfigDto(
                        Species: stored.Species,
                        LastModifiedAt: stored.LastModifiedAt,
                        LastModifiedBy: stored.LastModifiedBy
                    );
                    return _cachedConfig;
                }
            }

            // Fallback to defaults
            var defaults = GetDefaultSpeciesConfig();
            _cachedConfig = new SpeciesBreedConfigDto(
                Species: defaults,
                LastModifiedAt: DateTimeOffset.UtcNow,
                LastModifiedBy: "System (Default Seeding)"
            );

            await SaveToFileAsync(_cachedConfig, cancellationToken);
            return _cachedConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load species and breed configuration. Using system defaults.");
            return new SpeciesBreedConfigDto(
                Species: GetDefaultSpeciesConfig(),
                LastModifiedAt: DateTimeOffset.UtcNow,
                LastModifiedBy: "System (Fallback)"
            );
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<SpeciesBreedConfigDto> UpdateConfigAsync(
        List<SpeciesConfigDto> species,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var updated = new SpeciesBreedConfigDto(
                Species: species,
                LastModifiedAt: DateTimeOffset.UtcNow,
                LastModifiedBy: modifiedBy
            );

            await SaveToFileAsync(updated, cancellationToken);
            _cachedConfig = updated;
            _logger.LogInformation("Species and breed configuration updated by {User}", modifiedBy);
            return updated;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<SpeciesBreedConfigDto> ResetToDefaultsAsync(string modifiedBy, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var defaults = GetDefaultSpeciesConfig();
            var updated = new SpeciesBreedConfigDto(
                Species: defaults,
                LastModifiedAt: DateTimeOffset.UtcNow,
                LastModifiedBy: modifiedBy
            );

            await SaveToFileAsync(updated, cancellationToken);
            _cachedConfig = updated;
            _logger.LogInformation("Species and breed configuration reset to defaults by {User}", modifiedBy);
            return updated;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SaveToFileAsync(SpeciesBreedConfigDto config, CancellationToken cancellationToken)
    {
        var stored = new StoredSpeciesBreedConfig
        {
            Species = config.Species,
            LastModifiedAt = config.LastModifiedAt,
            LastModifiedBy = config.LastModifiedBy
        };

        var json = JsonSerializer.Serialize(stored, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }

    public static List<SpeciesConfigDto> GetDefaultSpeciesConfig()
    {
        return new List<SpeciesConfigDto>
        {
            new(
                Name: "Canine",
                DisplayName: "Canine (Dog)",
                Icon: "dog",
                Breeds: new List<BreedConfigDto>
                {
                    new("Dachshund", "Small", "Chondrodystrophic", "High risk IVDD; modified spinal mechanics"),
                    new("Corgi (Pembroke / Cardigan)", "Small", "Chondrodystrophic", "Long back; low ground clearance"),
                    new("French Bulldog", "Small", "Brachycephalic", "Spinal hemivertebrae; heat sensitivity"),
                    new("Pug", "Small", "Brachycephalic", "Respiratory consideration; luxating patella risk"),
                    new("Cavalier King Charles Spaniel", "Small", "Standard", "Cardiac and Chiari monitoring"),
                    new("Yorkshire Terrier", "Toy", "Standard", "Patellar luxation prone"),
                    new("Chihuahua", "Toy", "Standard", "Fragile cervical mechanics"),
                    new("Poodle (Toy / Miniature)", "Toy", "Standard", "Lightweight; agile"),
                    new("Beagle", "Medium", "Standard", "Disc disease prone"),
                    new("Cocker Spaniel", "Medium", "Standard", "Ear/balance, orthopedic monitoring"),
                    new("Border Collie", "Medium", "Athletic", "High drive; agility & proprioception focus"),
                    new("Australian Shepherd", "Medium", "Athletic", "Agile; working canine"),
                    new("Bulldog (English)", "Medium", "Brachycephalic", "Cruciate ligament and joint loading"),
                    new("Boxer", "Medium", "Athletic", "Cardiomyopathy & spondylosis prone"),
                    new("Golden Retriever", "Large", "Athletic", "Hip/elbow dysplasia, cruciate tear risk"),
                    new("Labrador Retriever", "Large", "Athletic", "Weight management; common CCL tear"),
                    new("German Shepherd", "Large", "Athletic", "Lumbosacral stenosis, degenerative myelopathy"),
                    new("Belgian Malinois", "Large", "Athletic", "High performance rehabilitation"),
                    new("Rottweiler", "Large", "Heavy / Muscular", "High bone density; cruciate loading"),
                    new("Doberman Pinscher", "Large", "Athletic", "Cervical spondylomyelopathy (Wobbler) prone"),
                    new("Siberian Husky", "Large", "Athletic", "Endurance; gait conditioning"),
                    new("Poodle (Standard)", "Large", "Athletic", "Light frame; athletic"),
                    new("Bernese Mountain Dog", "Large", "Heavy", "Orthopedic arthritis monitoring"),
                    new("Great Dane", "Giant", "Giant", "Long levers; joint space stress"),
                    new("Irish Wolfhound", "Giant", "Giant", "Cardiovascular & mobility support"),
                    new("Newfoundland", "Giant", "Giant / Water", "Excellent hydrotherapy candidate"),
                    new("Saint Bernard", "Giant", "Giant", "Heavy joint loading"),
                    new("Mixed Breed / Crossbreed", "Medium", "Variable", "General companion canine")
                }
            ),
            new(
                Name: "Feline",
                DisplayName: "Feline (Cat)",
                Icon: "cat",
                Breeds: new List<BreedConfigDto>
                {
                    new("Domestic Shorthair", "Medium", "Standard", "General companion feline"),
                    new("Domestic Longhair", "Medium", "Standard", "General companion feline"),
                    new("Maine Coon", "Large", "Large Frame", "Hip dysplasia and HCM risk"),
                    new("British Shorthair", "Medium", "Stocky", "Sturdy frame"),
                    new("Ragdoll", "Medium", "Large Frame", "Relaxed muscle tone"),
                    new("Siamese", "Medium", "Slender / Athletic", "High activity; slender frame"),
                    new("Persian", "Medium", "Brachycephalic", "Respiratory management"),
                    new("Bengal", "Medium", "Athletic", "High mobility drive"),
                    new("Scottish Fold", "Medium", "Osteochondrodysplasia Prone", "Cartilage and joint pain monitoring"),
                    new("Sphynx", "Medium", "Slender", "Temperature sensitivity")
                }
            ),
            new(
                Name: "Equine",
                DisplayName: "Equine (Horse)",
                Icon: "horse",
                Breeds: new List<BreedConfigDto>
                {
                    new("Thoroughbred", "Large", "Sport / Racing", "High speed tendon/ligament rehabilitation"),
                    new("Warmblood", "Large", "Dressage / Jumping", "Hock & stifle loading; spinal mobilization"),
                    new("Quarter Horse", "Large", "Muscular / Stock", "Navicular and muscular conditioning"),
                    new("Arabian", "Medium", "Endurance", "Light framed athletic conditioning"),
                    new("Friesian", "Large", "Draft / Sport", "High stepping gait mechanics"),
                    new("Standardbred", "Large", "Harness / Racing", "Pacing/trotting stride rehab"),
                    new("Pony (Welsh / Shetland)", "Small", "Pony Frame", "Laminitis management"),
                    new("Draft (Clydesdale / Percheron / Shire)", "Giant", "Draft / Heavy", "Heavy mass joint support")
                }
            ),
            new(
                Name: "Avian",
                DisplayName: "Avian (Bird)",
                Icon: "bird",
                Breeds: new List<BreedConfigDto>
                {
                    new("Parrot (African Grey / Amazon / Macaw)", "Medium", "Standard", "Wing & perch balance rehab"),
                    new("Cockatiel / Parakeet / Conure", "Small", "Small", "Pododermatitis & perch mobility"),
                    new("Raptor / Bird of Prey", "Medium", "Sport / Working", "Flight muscle recovery")
                }
            ),
            new(
                Name: "Other",
                DisplayName: "Other / Exotic",
                Icon: "paw",
                Breeds: new List<BreedConfigDto>
                {
                    new("Rabbit (Domestic / Lop)", "Small", "Lagomorph", "Spinal fragility; splay leg"),
                    new("Guinea Pig", "Small", "Rodent", "Hypovitaminosis C & arthritis"),
                    new("Ferret", "Small", "Carnivore", "Spinal flexibility"),
                    new("Caprine / Goat", "Medium", "Ruminant", "Hoof & joint rehabilitation"),
                    new("Camelid (Alpaca / Llama)", "Large", "Camelid", "Stifle & gait conditioning")
                }
            )
        };
    }

    private class StoredSpeciesBreedConfig
    {
        public List<SpeciesConfigDto> Species { get; set; } = [];
        public DateTimeOffset? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
