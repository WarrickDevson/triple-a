import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/pet_avatar.dart';
import '../../../core/widgets/section_card.dart';
import '../models/pet.dart';
import '../models/shared_report_model.dart';
import '../providers/shared_reports_provider.dart';

class SharedDocumentDetailScreen extends ConsumerStatefulWidget {
  const SharedDocumentDetailScreen({
    super.key,
    required this.pet,
    required this.report,
  });

  final Pet pet;
  final SharedReportModel report;

  @override
  ConsumerState<SharedDocumentDetailScreen> createState() => _SharedDocumentDetailScreenState();
}

class _SharedDocumentDetailScreenState extends ConsumerState<SharedDocumentDetailScreen> {
  bool _isDownloading = false;

  IconData _getCategoryIcon(SharedReportModel report) {
    if (report.isSoapNote) return Icons.assignment_outlined;
    if (report.isClinicalReport) return Icons.picture_as_pdf_outlined;
    if (report.isHomeProgram) return Icons.fitness_center_outlined;
    if (report.reportType.toUpperCase().contains('REFERRAL')) return Icons.local_hospital_outlined;
    if (report.reportType.toUpperCase().contains('IMAGING')) return Icons.camera_alt_outlined;
    return Icons.description_outlined;
  }

  Color _getCategoryColor(SharedReportModel report) {
    if (report.isSoapNote) return AppColors.sage;
    if (report.isClinicalReport) return const Color(0xFF1E6E8E);
    if (report.isHomeProgram) return const Color(0xFF5E548E);
    if (report.reportType.toUpperCase().contains('REFERRAL')) return const Color(0xFFC2185B);
    if (report.reportType.toUpperCase().contains('IMAGING')) return const Color(0xFF00796B);
    return const Color(0xFFE65100);
  }

  Future<void> _handleDownload() async {
    setState(() => _isDownloading = true);
    try {
      bool success = false;
      if (widget.report.soapNoteId != null) {
        success = await ref
            .read(sharedReportsProvider.notifier)
            .downloadSoapNotePdf(widget.report.soapNoteId!, widget.pet.petName);
      } else {
        success = await ref
            .read(sharedReportsProvider.notifier)
            .downloadPetClinicalReport(widget.pet.petId, widget.pet.petName);
      }

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              success
                  ? 'Document downloaded successfully!'
                  : 'Document could not be downloaded from server.',
            ),
            backgroundColor: success ? const Color(0xFF2E7D32) : AppColors.alertRed,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isDownloading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final report = widget.report;
    final categoryColor = _getCategoryColor(report);
    final categoryIcon = _getCategoryIcon(report);
    final dateStr =
        '${report.sharedAtUtc.year}-${report.sharedAtUtc.month.toString().padLeft(2, '0')}-${report.sharedAtUtc.day.toString().padLeft(2, '0')}';

    return AppPageScaffold(
      title: 'Shared Clinical Document',
      actions: [
        IconButton(
          icon: const Icon(Icons.download_rounded),
          tooltip: 'Download File',
          onPressed: _isDownloading ? null : _handleDownload,
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
                  : const Icon(Icons.file_download_rounded, size: 22),
              label: Text(
                _isDownloading ? 'Downloading...' : 'Download Official ${report.categoryLabel}',
                style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 15),
              ),
            ),
          ),
        ),
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 16, 20, 120),
        children: [
          // Header Card: Patient, Clinician & Date
          SectionCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    PetAvatar(name: widget.pet.petName, species: widget.pet.species, size: 48),
                    const SizedBox(width: 14),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            widget.pet.petName,
                            style: const TextStyle(
                              fontWeight: FontWeight.w800,
                              fontSize: 18,
                              color: AppColors.navy,
                            ),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            '${widget.pet.species}${widget.pet.breed != null ? " · ${widget.pet.breed}" : ""}',
                            style: const TextStyle(fontSize: 12, color: AppColors.neutralMuted),
                          ),
                        ],
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                      decoration: BoxDecoration(
                        color: categoryColor.withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(10),
                        border: Border.all(color: categoryColor.withValues(alpha: 0.3)),
                      ),
                      child: Text(
                        dateStr,
                        style: TextStyle(
                          fontWeight: FontWeight.w800,
                          fontSize: 12,
                          color: categoryColor,
                        ),
                      ),
                    ),
                  ],
                ),
                const Divider(height: 24, color: AppColors.neutralGrey),
                Row(
                  children: [
                    Icon(Icons.medical_services_outlined, size: 16, color: categoryColor),
                    const SizedBox(width: 8),
                    Text(
                      'Shared by: ',
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
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                      decoration: BoxDecoration(
                        color: const Color(0xFFE8F5E9),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: const Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(Icons.check_circle_rounded, size: 12, color: Color(0xFF2E7D32)),
                          SizedBox(width: 4),
                          Text(
                            'Shared with Owner',
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
          ),

          const SizedBox(height: 16),

          // Document Metadata Card
          SectionCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: categoryColor.withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: Icon(categoryIcon, size: 20, color: categoryColor),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            report.categoryLabel,
                            style: TextStyle(
                              fontSize: 11,
                              fontWeight: FontWeight.w800,
                              color: categoryColor,
                              letterSpacing: 0.5,
                            ),
                          ),
                          Text(
                            report.title,
                            style: const TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w800,
                              color: AppColors.navy,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),

          const SizedBox(height: 16),

          // Clinical Summary & Details Card
          SectionCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Row(
                  children: [
                    Icon(Icons.notes_rounded, size: 18, color: AppColors.sage),
                    SizedBox(width: 8),
                    Text(
                      'Clinical Summary & Practitioner Notes',
                      style: TextStyle(
                        fontWeight: FontWeight.w800,
                        fontSize: 14,
                        color: AppColors.navy,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: AppColors.surface,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: AppColors.neutralGrey),
                  ),
                  child: Text(
                    report.summary != null && report.summary!.isNotEmpty
                        ? report.summary!
                        : 'No additional clinical remarks recorded for this document. You can download the complete attached file using the button below.',
                    style: const TextStyle(
                      fontSize: 13,
                      height: 1.55,
                      color: AppColors.neutralDark,
                    ),
                  ),
                ),
              ],
            ),
          ),

          const SizedBox(height: 16),

          // Help / Guidance Card
          SectionCard(
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Icon(Icons.info_outline_rounded, size: 20, color: Color(0xFF1E6E8E)),
                const SizedBox(width: 10),
                Expanded(
                  child: Text(
                    'This clinical document was officially prepared and shared by your animal physiotherapist for ${widget.pet.petName}. Use the download button below to save a copy for your veterinary records.',
                    style: TextStyle(
                      fontSize: 12,
                      height: 1.45,
                      color: Colors.grey.shade700,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
