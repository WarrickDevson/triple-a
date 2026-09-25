import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../services/app_update_service.dart';
import '../theme/app_colors.dart';

class AppUpdateBanner extends ConsumerWidget {
  const AppUpdateBanner({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final updateState = ref.watch(appUpdateStateProvider);

    // If no update is available or the user dismissed it, don't take up any space
    if (!updateState.isAvailable || updateState.isDismissed) {
      return const SizedBox.shrink();
    }

    final isDownloading = updateState.status == UpdateInstallStatus.downloading;
    final isDownloaded = updateState.status == UpdateInstallStatus.downloaded;

    return Container(
      width: double.infinity,
      decoration: BoxDecoration(
        color: isDownloaded ? const Color(0xFFEAF5EB) : AppColors.sageMuted,
        border: Border(
          bottom: BorderSide(
            color: isDownloaded ? AppColors.successGreen.withValues(alpha: 0.3) : AppColors.sage.withValues(alpha: 0.25),
            width: 1,
          ),
        ),
      ),
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
      child: SafeArea(
        top: false,
        bottom: false,
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(6),
              decoration: BoxDecoration(
                color: isDownloaded ? AppColors.successGreen.withValues(alpha: 0.15) : AppColors.sage.withValues(alpha: 0.15),
                shape: BoxShape.circle,
              ),
              child: Icon(
                isDownloaded
                    ? Icons.check_circle_outline_rounded
                    : (isDownloading ? Icons.downloading_rounded : Icons.system_update_rounded),
                size: 20,
                color: isDownloaded ? AppColors.successGreen : AppColors.sage,
              ),
            ),
            const SizedBox(width: 10),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    isDownloaded
                        ? 'Update ready to install'
                        : (isDownloading ? 'Downloading update...' : 'Update available'),
                    style: const TextStyle(
                      fontWeight: FontWeight.w700,
                      fontSize: 13,
                      color: AppColors.navy,
                    ),
                  ),
                  Text(
                    isDownloaded
                        ? 'Restart app to apply the new version.'
                        : (isDownloading
                            ? 'Google Play is downloading in background.'
                            : 'A new version of MoveWell is ready.'),
                    style: const TextStyle(
                      fontSize: 11,
                      color: AppColors.neutralMuted,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ),
            ),
            const SizedBox(width: 8),
            if (isDownloading)
              const SizedBox(
                width: 20,
                height: 20,
                child: CircularProgressIndicator(
                  strokeWidth: 2,
                  valueColor: AlwaysStoppedAnimation<Color>(AppColors.sage),
                ),
              )
            else if (isDownloaded)
              ElevatedButton(
                onPressed: () => AppUpdateService.completeUpdate(),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.successGreen,
                  foregroundColor: Colors.white,
                  elevation: 0,
                  visualDensity: VisualDensity.compact,
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                  textStyle: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(6)),
                ),
                child: const Text('Restart'),
              )
            else ...[
              ElevatedButton(
                onPressed: () => AppUpdateService.startUpdate(ref, context),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.sage,
                  foregroundColor: Colors.white,
                  elevation: 0,
                  visualDensity: VisualDensity.compact,
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                  textStyle: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(6)),
                ),
                child: const Text('Update'),
              ),
              IconButton(
                icon: const Icon(Icons.close_rounded, size: 18, color: AppColors.neutralMuted),
                visualDensity: VisualDensity.compact,
                padding: EdgeInsets.zero,
                constraints: const BoxConstraints(minWidth: 28, minHeight: 28),
                onPressed: () => ref.read(appUpdateStateProvider.notifier).dismiss(),
                tooltip: 'Dismiss update notice',
              ),
            ],
          ],
        ),
      ),
    );
  }
}
