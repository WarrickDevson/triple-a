import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../core/config/app_config.dart';
import '../../../core/theme/app_colors.dart';
import '../models/shared_report_model.dart';

class DocumentPreviewDialog extends StatelessWidget {
  const DocumentPreviewDialog({
    super.key,
    required this.report,
    this.onDownload,
  });

  final SharedReportModel report;
  final VoidCallback? onDownload;

  static Future<void> show(
    BuildContext context, {
    required SharedReportModel report,
    VoidCallback? onDownload,
  }) {
    return showDialog<void>(
      context: context,
      barrierDismissible: true,
      builder: (ctx) => DocumentPreviewDialog(
        report: report,
        onDownload: onDownload,
      ),
    );
  }

  String? get _resolvedFileUrl {
    final raw = report.fileUrl;
    if (raw == null || raw.trim().isEmpty) return null;
    if (raw.startsWith('http://') ||
        raw.startsWith('https://') ||
        raw.startsWith('blob:') ||
        raw.startsWith('data:')) {
      return raw;
    }
    final base = AppConfig.fromEnvironment().apiBaseUrl.replaceAll(RegExp(r'/+$'), '');
    return '$base${raw.startsWith('/') ? raw : '/$raw'}';
  }

  bool get _isImage {
    final type = report.fileType?.toLowerCase() ?? '';
    final url = (_resolvedFileUrl ?? report.title).toLowerCase();
    return type.startsWith('image/') ||
        url.contains('.png') ||
        url.contains('.jpg') ||
        url.contains('.jpeg') ||
        url.contains('.webp') ||
        url.contains('.gif') ||
        url.contains('.svg');
  }

  bool get _isPdf {
    final type = report.fileType?.toLowerCase() ?? '';
    final url = (_resolvedFileUrl ?? report.title).toLowerCase();
    return type.contains('pdf') || url.contains('.pdf');
  }

  Future<void> _openExternal(BuildContext context) async {
    final url = _resolvedFileUrl;
    if (url != null && url.isNotEmpty) {
      final uri = Uri.parse(url);
      try {
        final launched = await launchUrl(uri, mode: LaunchMode.externalApplication);
        if (!launched && context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Could not open document in external viewer.')),
          );
        }
      } catch (e) {
        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Unable to launch URL: $e')),
          );
        }
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final dateStr =
        '${report.sharedAtUtc.year}-${report.sharedAtUtc.month.toString().padLeft(2, '0')}-${report.sharedAtUtc.day.toString().padLeft(2, '0')}';
    final hasFile = _resolvedFileUrl != null && _resolvedFileUrl!.isNotEmpty;

    return Dialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      insetPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 24),
      clipBehavior: Clip.antiAlias,
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 720, maxHeight: 760),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // 1. Header
            Container(
              padding: const EdgeInsets.fromLTRB(20, 16, 16, 14),
              decoration: const BoxDecoration(
                color: Colors.white,
                border: Border(bottom: BorderSide(color: AppColors.neutralGrey)),
              ),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    width: 42,
                    height: 42,
                    decoration: BoxDecoration(
                      color: AppColors.sageMuted,
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: const Icon(Icons.description_outlined, color: AppColors.sage, size: 22),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Flexible(
                              child: Text(
                                report.title,
                                style: const TextStyle(
                                  fontSize: 16,
                                  fontWeight: FontWeight.w800,
                                  color: AppColors.navy,
                                  height: 1.25,
                                ),
                                maxLines: 2,
                                overflow: TextOverflow.ellipsis,
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 4),
                        Wrap(
                          spacing: 6,
                          runSpacing: 4,
                          crossAxisAlignment: WrapCrossAlignment.center,
                          children: [
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 2),
                              decoration: BoxDecoration(
                                color: AppColors.sageMuted,
                                borderRadius: BorderRadius.circular(6),
                              ),
                              child: Text(
                                report.categoryLabel,
                                style: const TextStyle(
                                  fontSize: 10.5,
                                  fontWeight: FontWeight.w800,
                                  color: AppColors.sage,
                                ),
                              ),
                            ),
                            if (report.petName != null) ...[
                              Text('•', style: TextStyle(color: AppColors.neutralMuted.withValues(alpha: 0.6))),
                              Text(
                                report.petName!,
                                style: const TextStyle(
                                  fontSize: 11.5,
                                  fontWeight: FontWeight.w700,
                                  color: AppColors.navy,
                                ),
                              ),
                            ],
                            Text('•', style: TextStyle(color: AppColors.neutralMuted.withValues(alpha: 0.6))),
                            Text(
                              dateStr,
                              style: const TextStyle(fontSize: 11, color: AppColors.neutralMuted),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.close_rounded, size: 20, color: AppColors.neutralMuted),
                    onPressed: () => Navigator.of(context).pop(),
                  ),
                ],
              ),
            ),

            // 2. Preview Body Canvas
            Flexible(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    // A. Interactive Image Preview
                    if (hasFile && _isImage) ...[
                      Container(
                        constraints: const BoxConstraints(maxHeight: 380),
                        decoration: BoxDecoration(
                          color: AppColors.surface,
                          borderRadius: BorderRadius.circular(14),
                          border: Border.all(color: AppColors.neutralGrey),
                        ),
                        clipBehavior: Clip.antiAlias,
                        child: InteractiveViewer(
                          maxScale: 4.0,
                          child: Center(
                            child: Image.network(
                              _resolvedFileUrl!,
                              fit: BoxFit.contain,
                              loadingBuilder: (context, child, progress) {
                                if (progress == null) return child;
                                return const Center(
                                  child: Padding(
                                    padding: EdgeInsets.all(32),
                                    child: CircularProgressIndicator(strokeWidth: 2),
                                  ),
                                );
                              },
                              errorBuilder: (context, error, stackTrace) {
                                return Center(
                                  child: Padding(
                                    padding: const EdgeInsets.all(24),
                                    child: Column(
                                      mainAxisSize: MainAxisSize.min,
                                      children: [
                                        const Icon(Icons.broken_image_outlined,
                                            size: 44, color: AppColors.neutralMuted),
                                        const SizedBox(height: 8),
                                        const Text(
                                          'Could not load image preview directly.',
                                          style: TextStyle(
                                              fontSize: 12, color: AppColors.neutralDark),
                                        ),
                                        TextButton(
                                          onPressed: () => _openExternal(context),
                                          child: const Text('Open in external viewer'),
                                        ),
                                      ],
                                    ),
                                  ),
                                );
                              },
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(height: 8),
                      Center(
                        child: Text(
                          'Pinch or scroll to zoom · Double-tap to reset',
                          style: TextStyle(
                            fontSize: 11,
                            color: AppColors.neutralMuted.withValues(alpha: 0.8),
                          ),
                        ),
                      ),
                      const SizedBox(height: 16),
                    ]
                    // B. PDF File Card Preview Banner
                    else if (hasFile && _isPdf) ...[
                      Container(
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: const Color(0xFFF0F7FA),
                          borderRadius: BorderRadius.circular(14),
                          border: Border.all(color: const Color(0xFFC7E2EC)),
                        ),
                        child: Row(
                          children: [
                            Container(
                              padding: const EdgeInsets.all(10),
                              decoration: BoxDecoration(
                                color: const Color(0xFF1E6E8E).withValues(alpha: 0.12),
                                borderRadius: BorderRadius.circular(10),
                              ),
                              child: const Icon(Icons.picture_as_pdf_rounded,
                                  color: Color(0xFF1E6E8E), size: 28),
                            ),
                            const SizedBox(width: 14),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  const Text(
                                    'PDF Document Available',
                                    style: TextStyle(
                                      fontSize: 13.5,
                                      fontWeight: FontWeight.w800,
                                      color: Color(0xFF0F3E50),
                                    ),
                                  ),
                                  const SizedBox(height: 2),
                                  Text(
                                    report.fileSizeBytes != null
                                        ? '${(report.fileSizeBytes! / 1024).toStringAsFixed(1)} KB · Clinical Attachment'
                                        : 'Official Clinical PDF Document Attachment',
                                    style: const TextStyle(
                                      fontSize: 11.5,
                                      color: Color(0xFF336376),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            OutlinedButton.icon(
                              style: OutlinedButton.styleFrom(
                                foregroundColor: const Color(0xFF1E6E8E),
                                side: const BorderSide(color: Color(0xFF1E6E8E)),
                                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                                shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(8)),
                              ),
                              onPressed: () => _openExternal(context),
                              icon: const Icon(Icons.open_in_new_rounded, size: 15),
                              label: const Text('Open PDF',
                                  style: TextStyle(fontSize: 12, fontWeight: FontWeight.w700)),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 16),
                    ],

                    // C. Formatted Clinical Record Overview Card
                    Container(
                      padding: const EdgeInsets.all(18),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(14),
                        border: Border.all(color: AppColors.neutralGrey),
                        boxShadow: [
                          BoxStyle.subtle,
                        ],
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              const Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    'TRIPLE A VETERINARY PHYSIOTHERAPY',
                                    style: TextStyle(
                                      fontSize: 11,
                                      fontWeight: FontWeight.w800,
                                      letterSpacing: 0.8,
                                      color: AppColors.sage,
                                    ),
                                  ),
                                  Text(
                                    'Clinical Rehabilitation & Care Record',
                                    style: TextStyle(
                                      fontSize: 10.5,
                                      color: AppColors.neutralMuted,
                                    ),
                                  ),
                                ],
                              ),
                              Container(
                                padding:
                                    const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                decoration: BoxDecoration(
                                  color: const Color(0xFFE8F5E9),
                                  borderRadius: BorderRadius.circular(6),
                                  border: Border.all(color: const Color(0xFFA5D6A7)),
                                ),
                                child: const Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    Icon(Icons.verified_outlined,
                                        size: 13, color: Color(0xFF2E7D32)),
                                    SizedBox(width: 4),
                                    Text(
                                      'Verified',
                                      style: TextStyle(
                                        fontSize: 11,
                                        fontWeight: FontWeight.w800,
                                        color: Color(0xFF2E7D32),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ],
                          ),
                          const Divider(height: 24, color: AppColors.neutralGrey),

                          // Metadata Grid
                          Container(
                            padding: const EdgeInsets.all(12),
                            decoration: BoxDecoration(
                              color: AppColors.surface,
                              borderRadius: BorderRadius.circular(10),
                            ),
                            child: Row(
                              children: [
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      const Text('Patient Companion',
                                          style: TextStyle(
                                              fontSize: 11, color: AppColors.neutralMuted)),
                                      const SizedBox(height: 2),
                                      Text(
                                        report.petName ?? 'Companion Patient',
                                        style: const TextStyle(
                                            fontSize: 13,
                                            fontWeight: FontWeight.w700,
                                            color: AppColors.navy),
                                      ),
                                    ],
                                  ),
                                ),
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      const Text('Clinician',
                                          style: TextStyle(
                                              fontSize: 11, color: AppColors.neutralMuted)),
                                      const SizedBox(height: 2),
                                      Text(
                                        report.sharedByPhysioName,
                                        style: const TextStyle(
                                            fontSize: 13,
                                            fontWeight: FontWeight.w700,
                                            color: AppColors.navy),
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                          ),

                          const SizedBox(height: 16),

                          // Clinical Summary Section
                          const Text(
                            'Clinical Summary & Assessment Notes',
                            style: TextStyle(
                              fontSize: 12.5,
                              fontWeight: FontWeight.w800,
                              color: AppColors.navy,
                            ),
                          ),
                          const SizedBox(height: 6),
                          Text(
                            report.summary != null && report.summary!.trim().isNotEmpty
                                ? report.summary!
                                : 'This document represents an official veterinary rehabilitation record published for ${report.petName ?? 'your pet'}. Use the download options below to obtain the complete file or PDF document.',
                            style: const TextStyle(
                              fontSize: 13,
                              height: 1.5,
                              color: AppColors.neutralDark,
                            ),
                          ),

                          const SizedBox(height: 16),
                          Container(
                            padding: const EdgeInsets.all(10),
                            decoration: BoxDecoration(
                              color: AppColors.sageMuted.withValues(alpha: 0.4),
                              borderRadius: BorderRadius.circular(8),
                              border: Border.all(
                                  color: AppColors.sage.withValues(alpha: 0.25)),
                            ),
                            child: Row(
                              children: [
                                const Icon(Icons.info_outline_rounded,
                                    size: 16, color: AppColors.sage),
                                const SizedBox(width: 8),
                                Expanded(
                                  child: Text(
                                    'Record ID: #REP-${report.sharedReportId.toString().padLeft(4, '0')} · Confidential Patient Record',
                                    style: const TextStyle(
                                      fontSize: 11,
                                      fontWeight: FontWeight.w600,
                                      color: AppColors.navy,
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),

            // 3. Bottom Action Bar
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
              decoration: const BoxDecoration(
                color: Colors.white,
                border: Border(top: BorderSide(color: AppColors.neutralGrey)),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  if (hasFile)
                    TextButton.icon(
                      onPressed: () => _openExternal(context),
                      icon: const Icon(Icons.open_in_new_rounded, size: 16),
                      label: const Text('Open in Browser', style: TextStyle(fontSize: 13)),
                    ),
                  if (onDownload != null) ...[
                    const SizedBox(width: 8),
                    ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.sage,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                        shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(10)),
                      ),
                      onPressed: () {
                        Navigator.of(context).pop();
                        onDownload?.call();
                      },
                      icon: const Icon(Icons.download_rounded, size: 16),
                      label: const Text('Download File',
                          style: TextStyle(fontWeight: FontWeight.w700, fontSize: 13)),
                    ),
                  ],
                  const SizedBox(width: 8),
                  OutlinedButton(
                    style: OutlinedButton.styleFrom(
                      foregroundColor: AppColors.navy,
                      side: const BorderSide(color: AppColors.neutralGrey),
                      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                      shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(10)),
                    ),
                    onPressed: () => Navigator.of(context).pop(),
                    child: const Text('Close', style: TextStyle(fontSize: 13)),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class BoxStyle {
  static final subtle = BoxShadow(
    color: AppColors.navy.withValues(alpha: 0.04),
    blurRadius: 10,
    offset: const Offset(0, 4),
  );
}
