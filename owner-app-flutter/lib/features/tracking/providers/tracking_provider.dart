import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';

class TrackingState {
  const TrackingState({
    this.isSubmitting = false,
    this.error,
    this.lastSaved,
  });

  final bool isSubmitting;
  final String? error;
  final DateTime? lastSaved;
}

class TrackingNotifier extends StateNotifier<TrackingState> {
  TrackingNotifier(this._dio, this._petId) : super(const TrackingState());

  final Dio _dio;
  final int _petId;

  Future<void> submit({
    required int pain,
    required int energy,
    required int mobility,
    required int appetite,
    required int lameness,
  }) async {
    state = const TrackingState(isSubmitting: true);
    try {
      await _dio.post<void>(
        '/api/pets/$_petId/tracking',
        data: {
          'painScore': pain,
          'energyScore': energy,
          'mobilityScore': mobility,
          'appetiteScore': appetite,
          'lamenessScore': lameness,
        },
      );
      state = TrackingState(lastSaved: DateTime.now());
    } on DioException {
      state = const TrackingState(error: 'Unable to save tracking log. Please try again.');
    }
  }
}

final trackingProvider =
    StateNotifierProvider.family<TrackingNotifier, TrackingState, int>((ref, petId) {
  return TrackingNotifier(ref.read(authProvider.notifier).client, petId);
});
