import 'package:flutter/material.dart';
import '../theme/app_colors.dart';
import '../utils/media_url_resolver.dart';
import 'full_screen_image_viewer.dart';

class UserAvatar extends StatelessWidget {
  const UserAvatar({
    super.key,
    required this.name,
    this.roleTitle,
    this.imageUrl,
    this.size = 40,
    this.enableViewer = false,
    this.backgroundColor,
    this.textColor,
    this.onTap,
  });

  final String name;
  final String? roleTitle;
  final String? imageUrl;
  final double size;
  final bool enableViewer;
  final Color? backgroundColor;
  final Color? textColor;
  final VoidCallback? onTap;

  String _getInitials() {
    final parts = name.trim().split(RegExp(r'\s+'));
    if (parts.isEmpty || parts[0].isEmpty) return 'U';
    if (parts.length == 1) return parts[0][0].toUpperCase();
    return '${parts[0][0]}${parts[parts.length - 1][0]}'.toUpperCase();
  }

  Widget _buildFallback() {
    return Center(
      child: Text(
        _getInitials(),
        style: TextStyle(
          fontWeight: FontWeight.w700,
          fontSize: size * 0.38,
          color: textColor ?? AppColors.sage,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final hasImage = imageUrl != null && imageUrl!.trim().isNotEmpty;

    Widget avatar = Container(
      width: size,
      height: size,
      decoration: BoxDecoration(
        color: backgroundColor ?? AppColors.sageMuted,
        shape: BoxShape.circle,
        border: Border.all(
          color: (textColor ?? AppColors.sage).withValues(alpha: 0.25),
        ),
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
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: textColor ?? AppColors.sage,
                      ),
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
                subtitle: roleTitle,
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
