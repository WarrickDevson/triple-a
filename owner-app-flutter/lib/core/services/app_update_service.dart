import 'dart:io';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:in_app_update/in_app_update.dart';
import 'package:shared_preferences/shared_preferences.dart';

const String _updateDismissedTimestampKey = 'update_banner_dismissed_at';
const int _dismissCooldownHours = 24;

enum UpdateInstallStatus {
  idle,
  downloading,
  downloaded,
}

class AppUpdateState {
  final bool isAvailable;
  final UpdateInstallStatus status;
  final bool isDismissed;
  final int? availableVersionCode;

  const AppUpdateState({
    this.isAvailable = false,
    this.status = UpdateInstallStatus.idle,
    this.isDismissed = false,
    this.availableVersionCode,
  });

  AppUpdateState copyWith({
    bool? isAvailable,
    UpdateInstallStatus? status,
    bool? isDismissed,
    int? availableVersionCode,
  }) {
    return AppUpdateState(
      isAvailable: isAvailable ?? this.isAvailable,
      status: status ?? this.status,
      isDismissed: isDismissed ?? this.isDismissed,
      availableVersionCode: availableVersionCode ?? this.availableVersionCode,
    );
  }
}

class AppUpdateNotifier extends StateNotifier<AppUpdateState> {
  AppUpdateNotifier() : super(const AppUpdateState());

  void setAvailable({required int? versionCode}) {
    state = state.copyWith(
      isAvailable: true,
      availableVersionCode: versionCode,
      isDismissed: false,
    );
  }

  void setStatus(UpdateInstallStatus status) {
    state = state.copyWith(status: status);
  }

  Future<void> dismiss() async {
    state = state.copyWith(isDismissed: true);
    final prefs = await SharedPreferences.getInstance();
    await prefs.setInt(
      _updateDismissedTimestampKey,
      DateTime.now().millisecondsSinceEpoch,
    );
  }

  void reset() {
    state = const AppUpdateState();
  }
}

final appUpdateStateProvider =
    StateNotifierProvider<AppUpdateNotifier, AppUpdateState>((ref) {
  return AppUpdateNotifier();
});

class AppUpdateService {
  static bool _isChecking = false;

  /// Silently checks for updates in the background on startup.
  /// If an update is available and not recently dismissed, it informs the Riverpod
  /// state to show an unobtrusive banner WITHOUT popping up any dialog over user content.
  static Future<void> silentCheck(WidgetRef ref) async {
    if (kIsWeb || !Platform.isAndroid) return;
    if (_isChecking) return;
    _isChecking = true;

    try {
      // Check if user dismissed the update banner in the last 24 hours
      final prefs = await SharedPreferences.getInstance();
      final lastDismissedMs = prefs.getInt(_updateDismissedTimestampKey);
      if (lastDismissedMs != null) {
        final elapsed = DateTime.now().difference(
          DateTime.fromMillisecondsSinceEpoch(lastDismissedMs),
        );
        if (elapsed.inHours < _dismissCooldownHours) {
          // Cooldown active, don't show banner
          _isChecking = false;
          return;
        }
      }

      final AppUpdateInfo updateInfo = await InAppUpdate.checkForUpdate();

      if (updateInfo.updateAvailability == UpdateAvailability.updateAvailable) {
        ref.read(appUpdateStateProvider.notifier).setAvailable(
              versionCode: updateInfo.availableVersionCode,
            );
      } else if (updateInfo.updateAvailability ==
          UpdateAvailability.developerTriggeredUpdateInProgress) {
        ref.read(appUpdateStateProvider.notifier).setStatus(
              UpdateInstallStatus.downloaded,
            );
      }
    } catch (e) {
      debugPrint('[AppUpdateService] Silent update check error: $e');
    } finally {
      _isChecking = false;
    }
  }

  /// Starts downloading the update when the user clicks 'Update' on the banner.
  static Future<void> startUpdate(WidgetRef ref, BuildContext context) async {
    if (kIsWeb || !Platform.isAndroid) return;

    try {
      ref.read(appUpdateStateProvider.notifier).setStatus(UpdateInstallStatus.downloading);

      final AppUpdateResult result = await InAppUpdate.startFlexibleUpdate();

      if (result == AppUpdateResult.success) {
        ref.read(appUpdateStateProvider.notifier).setStatus(UpdateInstallStatus.downloaded);
      } else if (result == AppUpdateResult.userDeniedUpdate) {
        ref.read(appUpdateStateProvider.notifier).setStatus(UpdateInstallStatus.idle);
      }
    } catch (e) {
      ref.read(appUpdateStateProvider.notifier).setStatus(UpdateInstallStatus.idle);
      debugPrint('[AppUpdateService] Update download failed: $e');
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Failed to download update: $e')),
        );
      }
    }
  }

  /// Restarts and applies the downloaded update.
  static Future<void> completeUpdate() async {
    if (kIsWeb || !Platform.isAndroid) return;
    try {
      await InAppUpdate.completeFlexibleUpdate();
    } catch (e) {
      debugPrint('[AppUpdateService] Complete update error: $e');
    }
  }

  /// Manual check triggered from settings/More screen.
  static Future<void> manualCheck({
    required BuildContext context,
    required WidgetRef ref,
  }) async {
    if (kIsWeb || !Platform.isAndroid) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('In-app updates are only available on Android devices via Google Play.'),
          ),
        );
      }
      return;
    }

    try {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Checking Google Play for updates...'),
            duration: Duration(seconds: 2),
          ),
        );
      }

      final AppUpdateInfo updateInfo = await InAppUpdate.checkForUpdate();

      if (updateInfo.updateAvailability == UpdateAvailability.updateAvailable) {
        // Un-dismiss and show banner
        ref.read(appUpdateStateProvider.notifier).setAvailable(
              versionCode: updateInfo.availableVersionCode,
            );
        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('A new version is available! Tap Update in the banner above.'),
              backgroundColor: Colors.teal,
            ),
          );
        }
      } else {
        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('You are using the latest version.'),
            ),
          );
        }
      }
    } catch (e) {
      debugPrint('[AppUpdateService] Manual check failed: $e');
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              kDebugMode
                  ? 'Update check requires installing from Google Play Track ($e)'
                  : 'Unable to check for updates right now.',
            ),
          ),
        );
      }
    }
  }
}
