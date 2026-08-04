import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../config/brand.dart';
import '../theme/app_colors.dart';

class BrandLogo extends StatelessWidget {
  const BrandLogo({
    super.key,
    this.size = 48,
    this.showTagline = false,
    this.light = false,
  });

  final double size;
  final bool showTagline;
  final bool light;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        _LogoImage(size: size),
        const SizedBox(width: 10),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              Brand.name,
              style: GoogleFonts.lora(
                fontSize: size * 0.38,
                fontWeight: FontWeight.w700,
                color: light ? Colors.white : AppColors.navy,
                height: 1.1,
              ),
            ),
            if (showTagline)
              Text(
                Brand.tagline,
                style: TextStyle(
                  fontSize: size * 0.2,
                  fontWeight: FontWeight.w600,
                  color: light
                      ? Colors.white.withValues(alpha: 0.75)
                      : AppColors.sage,
                  letterSpacing: 0.3,
                ),
              ),
          ],
        ),
      ],
    );
  }
}

class _LogoImage extends StatelessWidget {
  const _LogoImage({required this.size});

  final double size;

  @override
  Widget build(BuildContext context) {
    return Image.asset(
      Brand.logoAsset,
      width: size,
      height: size,
      fit: BoxFit.contain,
      errorBuilder: (context, error, stackTrace) => Container(
        width: size,
        height: size,
        decoration: BoxDecoration(
          color: AppColors.sageMuted,
          shape: BoxShape.circle,
          border: Border.all(color: AppColors.sage.withValues(alpha: 0.3)),
        ),
        child: Center(
          child: Text(
            'A',
            style: GoogleFonts.lora(
              fontSize: size * 0.45,
              fontWeight: FontWeight.w700,
              color: AppColors.sage,
            ),
          ),
        ),
      ),
    );
  }
}
