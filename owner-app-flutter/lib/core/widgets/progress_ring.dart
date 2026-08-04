import 'package:flutter/material.dart';
import '../theme/app_colors.dart';

class ProgressRing extends StatelessWidget {
  const ProgressRing({
    super.key,
    required this.percent,
    this.size = 100,
    this.strokeWidth = 8,
    this.label,
  });

  final double percent;
  final double size;
  final double strokeWidth;
  final String? label;

  @override
  Widget build(BuildContext context) {
    final clamped = percent.clamp(0, 100);
    return SizedBox(
      width: size,
      height: size,
      child: Stack(
        alignment: Alignment.center,
        children: [
          SizedBox(
            width: size,
            height: size,
            child: CircularProgressIndicator(
              value: clamped / 100,
              strokeWidth: strokeWidth,
              backgroundColor: AppColors.neutralGrey,
              color: AppColors.sage,
              strokeCap: StrokeCap.round,
            ),
          ),
          Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                '${clamped.round()}%',
                style: TextStyle(
                  fontSize: size * 0.22,
                  fontWeight: FontWeight.w800,
                  color: AppColors.navy,
                ),
              ),
              if (label != null)
                Text(
                  label!,
                  style: TextStyle(
                    fontSize: size * 0.11,
                    color: AppColors.neutralMuted,
                    fontWeight: FontWeight.w600,
                  ),
                ),
            ],
          ),
        ],
      ),
    );
  }
}
