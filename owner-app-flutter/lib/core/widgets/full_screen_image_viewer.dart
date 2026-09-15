import 'package:flutter/material.dart';
import '../config/app_config.dart';
import '../theme/app_colors.dart';

String resolveMediaUrl(String path) {
  if (path.startsWith('http://') || path.startsWith('https://')) return path;
  final base = AppConfig.fromEnvironment().apiBaseUrl.replaceAll(RegExp(r'/+$'), '');
  final cleanPath = path.startsWith('/') ? path : '/$path';
  return '$base$cleanPath';
}

void showFullScreenImageViewer(
  BuildContext context, {
  required String imageUrl,
  required String title,
  String? subtitle,
  String? heroTag,
}) {
  if (imageUrl.trim().isEmpty) return;

  Navigator.of(context).push(
    PageRouteBuilder(
      opaque: false,
      barrierDismissible: true,
      pageBuilder: (context, animation, secondaryAnimation) {
        return FadeTransition(
          opacity: animation,
          child: FullScreenImageViewer(
            imageUrl: imageUrl,
            title: title,
            subtitle: subtitle,
            heroTag: heroTag,
          ),
        );
      },
    ),
  );
}

class FullScreenImageViewer extends StatelessWidget {
  const FullScreenImageViewer({
    super.key,
    required this.imageUrl,
    required this.title,
    this.subtitle,
    this.heroTag,
  });

  final String imageUrl;
  final String title;
  final String? subtitle;
  final String? heroTag;

  @override
  Widget build(BuildContext context) {
    final fullUrl = resolveMediaUrl(imageUrl.trim());

    Widget imageWidget = Image.network(
      fullUrl,
      fit: BoxFit.contain,
      loadingBuilder: (context, child, progress) {
        if (progress == null) return child;
        final total = progress.expectedTotalBytes;
        final current = progress.cumulativeBytesLoaded;
        final percent = total != null && total > 0 ? (current / total) : null;
        return Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              CircularProgressIndicator(
                value: percent,
                strokeWidth: 3,
                color: AppColors.sage,
              ),
              const SizedBox(height: 12),
              Text(
                percent != null ? '${(percent * 100).toInt()}%' : 'Loading image...',
                style: const TextStyle(color: Colors.white70, fontSize: 12),
              ),
            ],
          ),
        );
      },
      errorBuilder: (context, error, stackTrace) {
        return const Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(Icons.broken_image_rounded, color: Colors.white54, size: 56),
              SizedBox(height: 12),
              Text(
                'Unable to display image.',
                style: TextStyle(color: Colors.white70, fontSize: 14),
              ),
            ],
          ),
        );
      },
    );

    if (heroTag != null) {
      imageWidget = Hero(tag: heroTag!, child: imageWidget);
    }

    return Scaffold(
      backgroundColor: Colors.black.withValues(alpha: 0.94),
      body: SafeArea(
        child: Stack(
          children: [
            // Centered Zoomable and Pannable Image
            Center(
              child: InteractiveViewer(
                minScale: 0.8,
                maxScale: 5.0,
                child: SizedBox(
                  width: double.infinity,
                  height: double.infinity,
                  child: Center(child: imageWidget),
                ),
              ),
            ),

            // Top Header Bar
            Positioned(
              top: 0,
              left: 0,
              right: 0,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                decoration: const BoxDecoration(
                  gradient: LinearGradient(
                    begin: Alignment.topCenter,
                    end: Alignment.bottomCenter,
                    colors: [Colors.black87, Colors.transparent],
                  ),
                ),
                child: Row(
                  children: [
                    IconButton(
                      icon: const CircleAvatar(
                        radius: 18,
                        backgroundColor: Colors.white24,
                        child: Icon(Icons.arrow_back, color: Colors.white, size: 20),
                      ),
                      onPressed: () => Navigator.of(context).pop(),
                      tooltip: 'Close',
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Text(
                            title,
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                          if (subtitle != null && subtitle!.isNotEmpty)
                            Text(
                              subtitle!,
                              style: const TextStyle(
                                color: Colors.white70,
                                fontSize: 12,
                              ),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                        ],
                      ),
                    ),
                    IconButton(
                      icon: const CircleAvatar(
                        radius: 18,
                        backgroundColor: Colors.white24,
                        child: Icon(Icons.close, color: Colors.white, size: 20),
                      ),
                      onPressed: () => Navigator.of(context).pop(),
                      tooltip: 'Close',
                    ),
                  ],
                ),
              ),
            ),

            // Bottom Hint
            const Positioned(
              bottom: 16,
              left: 0,
              right: 0,
              child: Center(
                child: Text(
                  'Pinch to zoom · Drag to pan',
                  style: TextStyle(
                    color: Colors.white38,
                    fontSize: 11,
                    letterSpacing: 0.5,
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
