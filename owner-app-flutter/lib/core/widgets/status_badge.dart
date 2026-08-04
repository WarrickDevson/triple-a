import 'package:flutter/material.dart';
import '../theme/app_colors.dart';

enum StatusBadgeVariant { inProgress, completed, pending, stable, atRisk }

class StatusBadge extends StatelessWidget {
  const StatusBadge({
    super.key,
    required this.label,
    this.variant = StatusBadgeVariant.inProgress,
  });

  final String label;
  final StatusBadgeVariant variant;

  @override
  Widget build(BuildContext context) {
    final (bg, fg) = switch (variant) {
      StatusBadgeVariant.inProgress => (AppColors.sageMuted, AppColors.sage),
      StatusBadgeVariant.completed => (const Color(0xFFE6F4EA), AppColors.successGreen),
      StatusBadgeVariant.pending => (const Color(0xFFF0F1EE), AppColors.neutralMuted),
      StatusBadgeVariant.stable => (const Color(0xFFF0F1EE), AppColors.neutralMuted),
      StatusBadgeVariant.atRisk => (const Color(0xFFFEF3E2), const Color(0xFFB45309)),
    };

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(
        label,
        style: TextStyle(
          color: fg,
          fontSize: 11,
          fontWeight: FontWeight.w700,
          letterSpacing: 0.2,
        ),
      ),
    );
  }
}
