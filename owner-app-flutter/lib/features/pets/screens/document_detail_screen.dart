import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../core/config/app_config.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/pet_avatar.dart';
import '../../../core/widgets/section_card.dart';
import '../../../core/utils/south_africa_time.dart';
import '../models/pet.dart';
import '../models/shared_report_model.dart';
import '../providers/pets_provider.dart';
import '../providers/shared_reports_provider.dart';

class DocumentDetailScreen extends ConsumerStatefulWidget {
  const DocumentDetailScreen({
    super.key,
    required this.report,
    this.pet,
    this.onDownload,
  });

  final SharedReportModel report;
  final Pet? pet;
  final VoidCallback? onDownload;

  @override
  ConsumerState<DocumentDetailScreen> createState() => _DocumentDetailScreenState();
}

class _DocumentDetailScreenState extends ConsumerState<DocumentDetailScreen> {
  bool _isDownloading = false;

  String? get _resolvedFileUrl {
    final raw = widget.report.fileUrl;
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
    final type = widget.report.fileType?.toLowerCase() ?? '';
    final url = (_resolvedFileUrl ?? widget.report.title).toLowerCase();
    return type.startsWith('image/') ||
        url.contains('.png') ||
        url.contains('.jpg') ||
        url.contains('.jpeg') ||
        url.contains('.webp') ||
        url.contains('.gif') ||
        url.contains('.svg');
  }

  bool get _isPdf {
    final type = widget.report.fileType?.toLowerCase() ?? '';
    final url = (_resolvedFileUrl ?? widget.report.title).toLowerCase();
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
            SnackBar(content: Text('Unable to launch viewer: $e')),
          );
        }
      }
    }
  }

  Future<void> _handleDownload() async {
    if (widget.onDownload != null) {
      widget.onDownload!();
      return;
    }

    setState(() => _isDownloading = true);
    try {
      final petName = widget.pet?.petName ?? widget.report.petName ?? 'Companion';
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Downloading ${widget.report.title}...'),
          duration: const Duration(seconds: 1),
        ),
      );

      final success = await ref.read(sharedReportsProvider.notifier).downloadSharedReport(
            widget.report.sharedReportId,
            petName,
            widget.report.title,
          );

      if (!success && widget.report.isClinicalReport && mounted) {
        await ref.read(sharedReportsProvider.notifier).downloadPetClinicalReport(
              widget.report.petId,
              petName,
            );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Download failed: $e')),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isDownloading = false);
      }
    }
  }

  Color _getCategoryColor() {
    if (widget.report.isSoapNote) return AppColors.sage;
    if (widget.report.isClinicalReport) return const Color(0xFF1E6E8E);
    if (widget.report.isHomeProgram) return const Color(0xFF5E548E);
    return const Color(0xFFE65100);
  }

  IconData _getCategoryIcon() {
    if (widget.report.isSoapNote) return Icons.assignment_outlined;
    if (widget.report.isClinicalReport) return Icons.picture_as_pdf_outlined;
    if (widget.report.isHomeProgram) return Icons.fitness_center_outlined;
    if (widget.report.reportType.toUpperCase().contains('REFERRAL')) return Icons.local_hospital_outlined;
    if (widget.report.reportType.toUpperCase().contains('IMAGING')) return Icons.camera_alt_outlined;
    return Icons.description_outlined;
  }

  @override
  Widget build(BuildContext context) {
    final report = widget.report;
    final pets = ref.watch(petsProvider).pets;
    final pet = widget.pet ??
        (pets.any((p) => p.petId == report.petId)
            ? pets.firstWhere((p) => p.petId == report.petId)
            : Pet(
                petId: report.petId,
                ownerId: 0,
                ownerName: 'Owner',
                petName: report.petName ?? 'Companion',
                species: 'Canine',
                medicalHistories: const [],
              ));

    final hasFile = _resolvedFileUrl != null && _resolvedFileUrl!.isNotEmpty;
    final categoryColor = _getCategoryColor();
    final categoryIcon = _getCategoryIcon();

    String pageTitle = 'Document Details';
    if (report.isClinicalReport) {
      pageTitle = 'Clinical Progress Report';
    } else if (report.isHomeProgram) {
      pageTitle = 'Home Care Plan';
    } else if (report.categoryLabel.isNotEmpty) {
      pageTitle = report.categoryLabel;
    }

    String downloadBtnLabel = 'Download Document';
    if (report.isClinicalReport) {
      downloadBtnLabel = 'Download Clinical Report';
    } else if (_isPdf) {
      downloadBtnLabel = 'Download PDF Report';
    } else if (hasFile) {
      downloadBtnLabel = 'Download File Attachment';
    }

    return AppPageScaffold(
      title: pageTitle,
      actions: [
        if (hasFile)
          IconButton(
            icon: const Icon(Icons.open_in_browser_rounded),
            tooltip: 'Open in Browser',
            onPressed: () => _openExternal(context),
          ),
        IconButton(
          icon: const Icon(Icons.download_rounded),
          tooltip: 'Download Document',
          onPressed: _handleDownload,
        ),
      ],
      bottomNavigationBar: Container(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 20),
        decoration: BoxDecoration(
          color: Colors.white,
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.08),
              blurRadius: 10,
              offset: const Offset(0, -3),
            ),
          ],
        ),
        child: SafeArea(
          child: SizedBox(
            height: 52,
            child: ElevatedButton.icon(
              style: ElevatedButton.styleFrom(
                backgroundColor: categoryColor,
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                elevation: 2,
              ),
              onPressed: _isDownloading ? null : _handleDownload,
              icon: _isDownloading
                  ? const SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                    )
                  : Icon(
                      _isPdf ? Icons.picture_as_pdf_rounded : Icons.download_rounded,
                      size: 22,
                    ),
              label: Text(
                _isDownloading ? 'Downloading...' : downloadBtnLabel,
                style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 15),
              ),
            ),
          ),
        ),
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 16, 20, 120),
        children: [
          // 1. Header Card (Patient, Clinician, Date & Verified Badge)
          _buildHeaderCard(pet, report),

          const SizedBox(height: 16),

          // 2. Interactive Image Preview (if image)
          if (hasFile && _isImage) ...[
            _buildImageCard(context),
            const SizedBox(height: 16),
          ],

          // 3. PDF Document Attachment Card (if PDF)
          if (hasFile && _isPdf) ...[
            _buildPdfCard(context),
            const SizedBox(height: 16),
          ],

          // 4. Document Overview & Title Card
          _buildDocumentOverviewCard(report, categoryColor, categoryIcon),

          const SizedBox(height: 16),

          // 5. Clinical Summary & Content Section Card
          _buildClinicalSummaryCard(report, categoryColor),

          const SizedBox(height: 16),

          // 6. Security / Practice Record Audit Tag Card
          _buildAuditFooterCard(report),
        ],
      ),
    );
  }

  Widget _buildHeaderCard(Pet pet, SharedReportModel report) {
    final dateStr = SouthAfricaTime.toDateString(report.sharedAtUtc);

    return SectionCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              PetAvatar(name: pet.petName, species: pet.species, size: 48),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      pet.petName,
                      style: const TextStyle(
                        fontWeight: FontWeight.w800,
                        fontSize: 18,
                        color: AppColors.navy,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 2),
                    Text(
                      '${pet.species}${pet.breed != null && pet.breed!.isNotEmpty ? " · ${pet.breed}" : ""}',
                      style: const TextStyle(fontSize: 12, color: AppColors.neutralMuted),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                decoration: BoxDecoration(
                  color: AppColors.sageMuted,
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(color: AppColors.sage.withValues(alpha: 0.3)),
                ),
                child: Text(
                  dateStr,
                  style: const TextStyle(
                    fontWeight: FontWeight.w800,
                    fontSize: 12,
                    color: AppColors.sage,
                  ),
                ),
              ),
            ],
          ),
          const Divider(height: 24, color: AppColors.neutralGrey),
          Row(
            children: [
              const Icon(Icons.medical_services_outlined, size: 16, color: AppColors.sage),
              const SizedBox(width: 8),
              Text(
                'Attending Clinician: ',
                style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
              ),
              Expanded(
                child: Text(
                  report.sharedByPhysioName,
                  style: const TextStyle(
                    fontWeight: FontWeight.w700,
                    fontSize: 12,
                    color: AppColors.navy,
                  ),
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                decoration: BoxDecoration(
                  color: const Color(0xFFE8F5E9),
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: const Color(0xFFA5D6A7)),
                ),
                child: const Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.check_circle_rounded, size: 12, color: Color(0xFF2E7D32)),
                    SizedBox(width: 4),
                    Text(
                      'Verified Report',
                      style: TextStyle(
                        fontSize: 10,
                        fontWeight: FontWeight.bold,
                        color: Color(0xFF2E7D32),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildImageCard(BuildContext context) {
    return SectionCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Row(
                children: [
                  Icon(Icons.image_outlined, size: 18, color: AppColors.sage),
                  SizedBox(width: 8),
                  Text(
                    'Clinical Image Attachment',
                    style: TextStyle(
                      fontWeight: FontWeight.w800,
                      fontSize: 14,
                      color: AppColors.navy,
                    ),
                  ),
                ],
              ),
              TextButton.icon(
                style: TextButton.styleFrom(
                  visualDensity: VisualDensity.compact,
                  foregroundColor: AppColors.sage,
                ),
                onPressed: () => _openExternal(context),
                icon: const Icon(Icons.open_in_new_rounded, size: 14),
                label: const Text('Open Full', style: TextStyle(fontSize: 11.5, fontWeight: FontWeight.w700)),
              ),
            ],
          ),
          const SizedBox(height: 12),
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
                        child: CircularProgressIndicator(strokeWidth: 2, color: AppColors.sage),
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
                            const Icon(Icons.broken_image_outlined, size: 44, color: AppColors.neutralMuted),
                            const SizedBox(height: 8),
                            const Text(
                              'Could not render image preview.',
                              style: TextStyle(fontSize: 12, color: AppColors.neutralDark),
                            ),
                            TextButton(
                              onPressed: () => _openExternal(context),
                              child: const Text('Open in external browser'),
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
        ],
      ),
    );
  }

  Widget _buildPdfCard(BuildContext context) {
    return Container(
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
            child: const Icon(Icons.picture_as_pdf_rounded, color: Color(0xFF1E6E8E), size: 28),
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
                  widget.report.fileSizeBytes != null
                      ? '${(widget.report.fileSizeBytes! / 1024).toStringAsFixed(1)} KB · Clinical PDF'
                      : 'Official Clinical PDF Document Attachment',
                  style: const TextStyle(
                    fontSize: 11.5,
                    color: Color(0xFF336376),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: 8),
          OutlinedButton.icon(
            style: OutlinedButton.styleFrom(
              foregroundColor: const Color(0xFF1E6E8E),
              side: const BorderSide(color: Color(0xFF1E6E8E)),
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
            onPressed: () => _openExternal(context),
            icon: const Icon(Icons.open_in_new_rounded, size: 15),
            label: const Text('Open PDF', style: TextStyle(fontSize: 12, fontWeight: FontWeight.w700)),
          ),
        ],
      ),
    );
  }

  Widget _buildDocumentOverviewCard(SharedReportModel report, Color categoryColor, IconData categoryIcon) {
    return SectionCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Category Chip & Document Type
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
            decoration: BoxDecoration(
              color: categoryColor.withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(8),
              border: Border.all(color: categoryColor.withValues(alpha: 0.3)),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(categoryIcon, size: 14, color: categoryColor),
                const SizedBox(width: 6),
                Text(
                  report.categoryLabel,
                  style: TextStyle(
                    fontSize: 11.5,
                    fontWeight: FontWeight.w800,
                    color: categoryColor,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 12),

          // Title
          Text(
            report.title,
            style: const TextStyle(
              fontWeight: FontWeight.w800,
              fontSize: 17,
              color: AppColors.navy,
              height: 1.3,
            ),
          ),
          const SizedBox(height: 6),

          Text(
            'Published by ${report.sharedByPhysioName} for ${report.petName ?? 'Companion'}',
            style: const TextStyle(fontSize: 12.5, color: AppColors.neutralMuted),
          ),
        ],
      ),
    );
  }

  Widget _buildClinicalSummaryCard(SharedReportModel report, Color categoryColor) {
    return SectionCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 28,
                height: 28,
                decoration: BoxDecoration(
                  color: categoryColor,
                  borderRadius: BorderRadius.circular(8),
                ),
                alignment: Alignment.center,
                child: const Icon(Icons.description_outlined, color: Colors.white, size: 16),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      report.isClinicalReport
                          ? 'Clinical Findings & Progress Summary'
                          : (report.isHomeProgram
                              ? 'Care Plan & Exercise Instructions'
                              : 'Clinical Summary & Assessment Notes'),
                      style: const TextStyle(
                        fontWeight: FontWeight.w800,
                        fontSize: 14.5,
                        color: AppColors.navy,
                      ),
                    ),
                    const Text(
                      'Official notes recorded by attending physiotherapist',
                      style: TextStyle(
                        fontSize: 11,
                        color: AppColors.neutralMuted,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              color: AppColors.surface,
              borderRadius: BorderRadius.circular(10),
              border: Border.all(color: AppColors.neutralGrey),
            ),
            child: Text(
              report.summary != null && report.summary!.trim().isNotEmpty
                  ? report.summary!
                  : 'This document represents an official veterinary rehabilitation and physiotherapy record published for ${report.petName ?? 'your pet'}. Use the download options below to obtain the complete file or PDF document.',
              style: const TextStyle(
                fontSize: 13.5,
                height: 1.55,
                color: AppColors.neutralDark,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAuditFooterCard(SharedReportModel report) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: AppColors.sageMuted.withValues(alpha: 0.4),
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: AppColors.sage.withValues(alpha: 0.25)),
      ),
      child: Row(
        children: [
          const Icon(Icons.shield_outlined, size: 18, color: AppColors.sage),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              'Record ID: #REP-${report.sharedReportId.toString().padLeft(4, '0')} · Confidential Patient Record · Triple A Veterinary Physiotherapy',
              style: const TextStyle(
                fontSize: 11,
                fontWeight: FontWeight.w600,
                color: AppColors.navy,
                height: 1.35,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
