import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';
import '../models/pet.dart';

class PetsState {
  const PetsState({
    this.pets = const [],
    this.isLoading = false,
    this.error,
  });

  final List<Pet> pets;
  final bool isLoading;
  final String? error;
}

class PetsNotifier extends StateNotifier<PetsState> {
  PetsNotifier(this._dio, this._ownerId) : super(const PetsState()) {
    if (_ownerId != null) {
      loadPets();
    }
  }

  final Dio _dio;
  final int? _ownerId;

  Future<void> loadPets({bool force = false}) async {
    if (_ownerId == null) return;
    if (state.pets.isNotEmpty && !force) return;

    state = const PetsState(isLoading: true);
    try {
      final response = await _dio.get<List<dynamic>>('/api/pets/owner/$_ownerId');
      final pets = response.data!
          .map((item) => Pet.fromJson(item as Map<String, dynamic>))
          .toList();
      state = PetsState(pets: pets);
    } on DioException {
      state = const PetsState(error: 'Unable to load pets.');
    }
  }

  Future<bool> createPet({
    required String petName,
    required String species,
    String? breed,
    DateTime? birthDate,
    double? weightKg,
    String? diagnosis,
    String? injuryOrCondition,
  }) async {
    state = PetsState(pets: state.pets, isLoading: true);
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/pets',
        data: {
          'petName': petName,
          'species': species,
          if (breed != null && breed.isNotEmpty) 'breed': breed,
          if (birthDate != null) 'birthDate': _formatDate(birthDate),
          if (weightKg != null) 'weightKg': weightKg,
          if (diagnosis != null && diagnosis.isNotEmpty)
            'initialMedicalHistory': {
              'diagnosis': diagnosis,
              if (injuryOrCondition != null && injuryOrCondition.isNotEmpty)
                'injuryOrCondition': injuryOrCondition,
            },
        },
      );
      final pet = Pet.fromJson(response.data!);
      state = PetsState(pets: [pet, ...state.pets]);
      return true;
    } on DioException {
      state = PetsState(pets: state.pets, error: 'Unable to create pet profile.');
      return false;
    }
  }

  String _formatDate(DateTime date) {
    final month = date.month.toString().padLeft(2, '0');
    final day = date.day.toString().padLeft(2, '0');
    return '${date.year}-$month-$day';
  }
}

final petsProvider = StateNotifierProvider<PetsNotifier, PetsState>((ref) {
  final auth = ref.watch(authProvider);
  final authNotifier = ref.read(authProvider.notifier);
  return PetsNotifier(authNotifier.client, auth.user?.userId);
});
