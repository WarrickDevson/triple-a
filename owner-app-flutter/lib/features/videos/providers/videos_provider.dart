import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';
import '../models/video_submission.dart';

class VideosState {
  const VideosState({
    this.submissions = const [],
    this.isLoading = false,
    this.error,
  });

  final List<VideoSubmission> submissions;
  final bool isLoading;
  final String? error;
}

class VideosNotifier extends StateNotifier<VideosState> {
  VideosNotifier(this._dio) : super(const VideosState());

  final Dio _dio;

  Future<void> loadForPet(int petId) async {
    state = const VideosState(isLoading: true);
    try {
      final response = await _dio.get<dynamic>('/api/pets/$petId/videos');
      final data = response.data;
      final List<VideoSubmission> submissions = [];
      if (data is List) {
        for (final item in data) {
          if (item is Map<String, dynamic>) {
            submissions.add(VideoSubmission.fromJson(item));
          } else if (item is Map) {
            submissions.add(VideoSubmission.fromJson(Map<String, dynamic>.from(item)));
          }
        }
      }
      state = VideosState(submissions: submissions);
    } catch (e) {
      state = const VideosState(error: 'Unable to load video feedback.');
    }
  }

  Future<bool> updateVideo({
    required int petId,
    required int videoId,
    String? title,
    String? notes,
  }) async {
    try {
      await _dio.put(
        '/api/pets/$petId/videos/$videoId',
        data: {
          'title': title,
          'notes': notes,
        },
      );
      await loadForPet(petId);
      return true;
    } catch (_) {
      return false;
    }
  }

  Future<bool> deleteVideo({
    required int petId,
    required int videoId,
  }) async {
    try {
      await _dio.delete('/api/pets/$petId/videos/$videoId');
      await loadForPet(petId);
      return true;
    } catch (_) {
      return false;
    }
  }
}

final videosProvider = StateNotifierProvider<VideosNotifier, VideosState>((ref) {
  final authNotifier = ref.read(authProvider.notifier);
  return VideosNotifier(authNotifier.client);
});
