import 'package:flutter/material.dart';
import '../theme/app_colors.dart';

class PetAvatar extends StatelessWidget {
  const PetAvatar({
    super.key,
    required this.name,
    this.size = 48,
    this.species,
  });

  final String name;
  final double size;
  final String? species;

  IconData get _icon => switch (species?.toLowerCase()) {
        'feline' || 'cat' => Icons.pets,
        'equine' || 'horse' => Icons.agriculture_outlined,
        'avian' || 'bird' => Icons.flutter_dash,
        _ => Icons.pets_rounded,
      };

  @override
  Widget build(BuildContext context) {
    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(
        color: AppColors.sageMuted,
        shape: BoxShape.circle,
        border: Border.all(color: AppColors.sage.withValues(alpha: 0.25)),
      ),
      child: Icon(_icon, color: AppColors.sage, size: size * 0.45),
    );
  }
}
