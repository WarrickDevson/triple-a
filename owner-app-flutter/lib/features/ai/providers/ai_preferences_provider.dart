import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

class AiPreferencesState {
  const AiPreferencesState({
    this.sharePetData = false,
    this.isLoading = true,
  });

  final bool sharePetData;
  final bool isLoading;

  AiPreferencesState copyWith({
    bool? sharePetData,
    bool? isLoading,
  }) {
    return AiPreferencesState(
      sharePetData: sharePetData ?? this.sharePetData,
      isLoading: isLoading ?? this.isLoading,
    );
  }
}

class AiPreferencesNotifier extends StateNotifier<AiPreferencesState> {
  AiPreferencesNotifier() : super(const AiPreferencesState()) {
    _loadPreferences();
  }

  static const _prefKey = 'ai_share_pet_data';

  Future<void> _loadPreferences() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final sharePetData = prefs.getBool(_prefKey) ?? false;
      state = state.copyWith(sharePetData: sharePetData, isLoading: false);
    } catch (_) {
      state = state.copyWith(isLoading: false);
    }
  }

  Future<void> setSharePetData(bool enabled) async {
    state = state.copyWith(sharePetData: enabled);
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool(_prefKey, enabled);
    } catch (_) {
      // Ignore write errors to keep UI responsive
    }
  }

  Future<void> toggleSharePetData() async {
    await setSharePetData(!state.sharePetData);
  }
}

final aiPreferencesProvider =
    StateNotifierProvider<AiPreferencesNotifier, AiPreferencesState>((ref) {
  return AiPreferencesNotifier();
});
