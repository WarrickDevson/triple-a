import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../config/brand.dart';
import '../theme/app_colors.dart';
import 'brand_logo.dart';

/// Cream page wash behind authenticated screens.
class PageWashBackground extends StatelessWidget {
  const PageWashBackground({super.key, required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [
            AppColors.pageWashTop,
            AppColors.pageWashMid,
            AppColors.neutralLight,
          ],
        ),
      ),
      child: child,
    );
  }
}

/// White card panel matching portal cards.
class AppPanel extends StatelessWidget {
  const AppPanel({
    super.key,
    required this.child,
    this.padding = const EdgeInsets.all(20),
    this.margin,
  });

  final Widget child;
  final EdgeInsetsGeometry padding;
  final EdgeInsetsGeometry? margin;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: margin,
      padding: padding,
      decoration: BoxDecoration(
        color: AppColors.card,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.panelBorder),
        boxShadow: const [
          BoxShadow(
            color: AppColors.panelShadow,
            blurRadius: 16,
            offset: Offset(0, 4),
          ),
        ],
      ),
      child: child,
    );
  }
}

class AppEmptyState extends StatelessWidget {
  const AppEmptyState({
    super.key,
    required this.icon,
    required this.title,
    this.message,
    this.action,
  });

  final IconData icon;
  final String title;
  final String? message;
  final Widget? action;

  @override
  Widget build(BuildContext context) {
    return AppPanel(
      padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 36),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            width: 64,
            height: 64,
            decoration: BoxDecoration(
              color: AppColors.sageMuted,
              shape: BoxShape.circle,
            ),
            child: const Icon(Icons.pets_rounded, size: 32, color: AppColors.sage),
          ),
          const SizedBox(height: 16),
          Text(
            title,
            textAlign: TextAlign.center,
            style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  color: AppColors.navy,
                  fontWeight: FontWeight.w800,
                ),
          ),
          if (message != null) ...[
            const SizedBox(height: 8),
            Text(
              message!,
              textAlign: TextAlign.center,
              style: TextStyle(
                color: AppColors.neutralMuted,
                height: 1.45,
              ),
            ),
          ],
          if (action != null) ...[
            const SizedBox(height: 20),
            action!,
          ],
        ],
      ),
    );
  }
}

/// Light header + washed body for tab screens and pushed routes.
class AppPageScaffold extends StatelessWidget {
  const AppPageScaffold({
    super.key,
    required this.title,
    required this.body,
    this.actions,
    this.floatingActionButton,
    this.leading,
    this.bottomNavigationBar,
    this.showBrand = false,
  });

  final String title;
  final Widget body;
  final List<Widget>? actions;
  final Widget? floatingActionButton;
  final Widget? leading;
  final Widget? bottomNavigationBar;
  final bool showBrand;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.surface,
      appBar: AppBar(
        title: showBrand
            ? const BrandLogo(size: 36, showTagline: true)
            : Text(title),
        leading: leading,
        actions: actions,
        toolbarHeight: 64,
      ),
      floatingActionButton: floatingActionButton,
      bottomNavigationBar: bottomNavigationBar,
      body: PageWashBackground(child: body),
    );
  }
}

/// Header row used inside shell tabs (no AppBar).
class ShellHeader extends StatelessWidget {
  const ShellHeader({
    super.key,
    required this.title,
    this.subtitle,
    this.actions,
    this.showLogo = true,
  });

  final String title;
  final String? subtitle;
  final List<Widget>? actions;
  final bool showLogo;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (showLogo) ...[
            const BrandLogo(size: 40),
            const SizedBox(width: 12),
          ],
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: GoogleFonts.lora(
                    fontSize: 22,
                    fontWeight: FontWeight.w700,
                    color: AppColors.navy,
                  ),
                ),
                if (subtitle != null)
                  Text(
                    subtitle!,
                    style: TextStyle(
                      color: AppColors.neutralMuted,
                      fontSize: 14,
                    ),
                  ),
              ],
            ),
          ),
          ...?actions,
        ],
      ),
    );
  }
}

/// Script tagline accent.
class BrandMotto extends StatelessWidget {
  const BrandMotto({super.key});

  @override
  Widget build(BuildContext context) {
    return Text(
      Brand.motto,
      style: GoogleFonts.caveat(
        fontSize: 18,
        fontWeight: FontWeight.w600,
        color: AppColors.sageLight,
      ),
    );
  }
}
