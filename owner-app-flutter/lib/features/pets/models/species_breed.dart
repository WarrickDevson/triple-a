class BreedConfig {
  const BreedConfig({
    required this.name,
    this.sizeCategory,
    this.conformation,
    this.notes,
  });

  final String name;
  final String? sizeCategory;
  final String? conformation;
  final String? notes;

  factory BreedConfig.fromJson(Map<String, dynamic> json) {
    return BreedConfig(
      name: json['name'] as String? ?? '',
      sizeCategory: json['sizeCategory'] as String?,
      conformation: json['conformation'] as String?,
      notes: json['notes'] as String?,
    );
  }
}

class SpeciesConfig {
  const SpeciesConfig({
    required this.name,
    required this.displayName,
    this.icon,
    this.breeds = const [],
  });

  final String name;
  final String displayName;
  final String? icon;
  final List<BreedConfig> breeds;

  factory SpeciesConfig.fromJson(Map<String, dynamic> json) {
    return SpeciesConfig(
      name: json['name'] as String? ?? '',
      displayName: json['displayName'] as String? ?? json['name'] as String? ?? '',
      icon: json['icon'] as String?,
      breeds: (json['breeds'] as List<dynamic>? ?? [])
          .map((b) => BreedConfig.fromJson(b as Map<String, dynamic>))
          .toList(),
    );
  }
}

const defaultSpeciesFallback = [
  SpeciesConfig(name: 'Canine', displayName: 'Canine (Dog)'),
  SpeciesConfig(name: 'Feline', displayName: 'Feline (Cat)'),
  SpeciesConfig(name: 'Equine', displayName: 'Equine (Horse)'),
  SpeciesConfig(name: 'Avian', displayName: 'Avian (Bird)'),
  SpeciesConfig(name: 'Other', displayName: 'Other / Exotic'),
];
