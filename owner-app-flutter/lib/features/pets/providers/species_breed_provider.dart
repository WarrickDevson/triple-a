import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';
import '../models/species_breed.dart';

class SpeciesBreedState {
  const SpeciesBreedState({
    this.species = defaultSpeciesFallback,
    this.isLoading = false,
    this.error,
  });

  final List<SpeciesConfig> species;
  final bool isLoading;
  final String? error;

  List<String> get speciesNames => species.map((s) => s.name).toList();

  List<BreedConfig> breedsForSpecies(String? speciesName) {
    if (speciesName == null || speciesName.isEmpty) return const [];
    final match = species.firstWhere(
      (s) => s.name.toLowerCase() == speciesName.trim().toLowerCase(),
      orElse: () => const SpeciesConfig(name: '', displayName: ''),
    );
    return match.breeds;
  }
}

class SpeciesBreedNotifier extends StateNotifier<SpeciesBreedState> {
  SpeciesBreedNotifier(this._dio) : super(const SpeciesBreedState()) {
    loadConfig();
  }

  final Dio _dio;

  Future<void> loadConfig({bool force = false}) async {
    state = SpeciesBreedState(species: state.species, isLoading: true);
    try {
      final response = await _dio.get<Map<String, dynamic>>('/api/species-breeds');
      final data = response.data;
      if (data != null && data['species'] is List) {
        final list = (data['species'] as List<dynamic>)
            .map((s) => SpeciesConfig.fromJson(s as Map<String, dynamic>))
            .toList();
        if (list.isNotEmpty) {
          state = SpeciesBreedState(species: list);
          return;
        }
      }
      state = SpeciesBreedState(species: state.species);
    } catch (e) {
      state = SpeciesBreedState(species: state.species, error: e.toString());
    }
  }
}

final speciesBreedProvider =
    StateNotifierProvider<SpeciesBreedNotifier, SpeciesBreedState>((ref) {
  final dio = ref.watch(authProvider.notifier).client;
  return SpeciesBreedNotifier(dio);
});
