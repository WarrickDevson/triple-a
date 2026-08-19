import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';
import '../models/owner_subjective_note.dart';

class OwnerNotesState {
  const OwnerNotesState({
    this.isSubmitting = false,
    this.success = false,
    this.error,
  });

  final bool isSubmitting;
  final bool success;
  final String? error;
}

class OwnerNotesNotifier extends StateNotifier<OwnerNotesState> {
  OwnerNotesNotifier(this._dio) : super(const OwnerNotesState());

  final Dio _dio;

  Future<bool> submitOwnerSubjectiveNote({
    required int petId,
    required String notes,
    int? painObserved,
    int? energyObserved,
  }) async {
    state = const OwnerNotesState(isSubmitting: true);
    try {
      final data = <String, dynamic>{
        'notes': notes,
      };
      if (painObserved != null) data['painObserved'] = painObserved;
      if (energyObserved != null) data['energyObserved'] = energyObserved;

      await _dio.post('/api/soap-notes/pet/$petId/owner-notes', data: data);
      state = const OwnerNotesState(success: true);
      return true;
    } catch (e) {
      state = OwnerNotesState(error: e.toString());
      return false;
    }
  }

  Future<List<OwnerSubjectiveNote>> fetchNotes(int petId) async {
    try {
      final response = await _dio.get('/api/soap-notes/pet/$petId/owner-notes');
      final list = (response.data as List? ?? [])
          .map((item) => OwnerSubjectiveNote.fromJson(item as Map<String, dynamic>))
          .toList();
      return list;
    } catch (_) {
      return [];
    }
  }
}

final ownerNotesProvider = StateNotifierProvider<OwnerNotesNotifier, OwnerNotesState>((ref) {
  final authNotifier = ref.read(authProvider.notifier);
  return OwnerNotesNotifier(authNotifier.client);
});

final ownerNotesListProvider = FutureProvider.family<List<OwnerSubjectiveNote>, int>((ref, petId) async {
  final notifier = ref.read(ownerNotesProvider.notifier);
  return notifier.fetchNotes(petId);
});
