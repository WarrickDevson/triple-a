import 'package:flutter/material.dart';
import '../theme/app_colors.dart';
import '../utils/media_url_resolver.dart';
import 'full_screen_image_viewer.dart';

class PetAvatar extends StatelessWidget {
  const PetAvatar({
    super.key,
    required this.name,
    this.size = 48,
    this.species,
    this.imageUrl,
    this.enableViewer = false,
    this.onTap,
  });

  final String name;
  final double size;
  final String? species;
  final String? imageUrl;
  final bool enableViewer;
  final VoidCallback? onTap;

  IconData get _icon => switch (species?.toLowerCase()) {
        'feline' || 'cat' => Icons.pets,
        'equine' || 'horse' => Icons.agriculture_outlined,
        'avian' || 'bird' => Icons.flutter_dash,
        _ => Icons.pets_rounded,
      };

  Widget _buildFallback() {
    return Icon(_icon, color: AppColors.sage, size: size * 0.45);
  }

  @override
  Widget build(BuildContext context) {
    final hasImage = imageUrl != null && imageUrl!.trim().isNotEmpty;

    Widget avatar = Container(
      width: size,
      height: size,
      decoration: BoxDecoration(
        color: AppColors.sageMuted,
        shape: BoxShape.circle,
        border: Border.all(color: AppColors.sage.withValues(alpha: 0.25)),
      ),
      child: ClipOval(
        child: hasImage
            ? Image.network(
                resolveMediaUrl(imageUrl!.trim()) ?? imageUrl!.trim(),
                width: size,
                height: size,
                fit: BoxFit.cover,
                errorBuilder: (_, _, _) => _buildFallback(),
                loadingBuilder: (context, child, progress) {
                  if (progress == null) return child;
                  return Center(
                    child: SizedBox(
                      width: size * 0.4,
                      height: size * 0.4,
                      child: const CircularProgressIndicator(strokeWidth: 2, color: AppColors.sage),
                    ),
                  );
                },
              )
            : _buildFallback(),
      ),
    );

    if (enableViewer && hasImage) {
      avatar = GestureDetector(
        onTap: onTap ??
            () {
              showFullScreenImageViewer(
                context,
                imageUrl: imageUrl!,
                title: name,
                subtitle: species,
              );
            },
        child: avatar,
      );
    } else if (onTap != null) {
      avatar = GestureDetector(onTap: onTap, child: avatar);
    }

    return avatar;
  }
}

