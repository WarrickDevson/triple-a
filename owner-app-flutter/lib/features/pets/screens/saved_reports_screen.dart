import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/section_card.dart';
import '../models/pet.dart';
import '../models/shared_report_model.dart';
import '../providers/shared_reports_provider.dart';
import 'soap_note_detail_screen.dart';

class SavedReportsScreen extends ConsumerStatefulWidget {
  const SavedReportsScreen({super.key, required this.pet});

  final Pet pet;

  @override
  ConsumerState<SavedReportsScreen> createState() => _SavedReportsScreenState();
}

class _SavedReportsScreenState extends ConsumerState<SavedReportsScreen> {
  String _selectedCategory = 'All';
  String _searchQuery = '';
  final TextEditingController _searchController = TextEditingController();

  final List<String> _categories = [
    'All',
    'SOAP Notes',
    'Clinical Reports',
    'Care Plans & Files',
  ];

  @override
  void initState() {
    super.initState();
    Future.microtask(() {
      ref.read(sharedReportsProvider.notifier).fetchSharedReports(widget.pet.petId);
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _refresh() async {
    await ref.read(sharedReportsProvider.notifier).fetchSharedReports(widget.pet.petId);
  }

  void _openPdfReport(int? soapNoteId) async {
    if (soapNoteId == null) return;
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Downloading SOAP Note PDF...'), duration: Duration(seconds: 1)),
    );
    final success = await ref
        .read(sharedReportsProvider.notifier)
        .downloadSoapNotePdf(soapNoteId, widget.pet.petName);

    if (!success && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not download PDF report from server.')),
      );
    }
  }

  void _downloadPetClinicalReport() async {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Downloading Clinical Progress Report...'), duration: Duration(seconds: 1)),
    );
    final success = await ref
        .read(sharedReportsProvider.notifier)
        .downloadPetClinicalReport(widget.pet.petId, widget.pet.petName);

    if (!success && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not download clinical progress report from server.')),
      );
    }
  }

  void _openSoapDetail(int? soapNoteId) {
    if (soapNoteId == null) return;
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => SoapNoteDetailScreen(
          pet: widget.pet,
          soapNoteId: soapNoteId,
        ),
      ),
    );
  }

  List<SharedReportModel> _filterReports(List<SharedReportModel> reports) {
    return reports.where((r) {
      if (_selectedCategory == 'SOAP Notes' && !r.isSoapNote) return false;
      if (_selectedCategory == 'Clinical Reports' && !r.isClinicalReport) return false;
      if (_selectedCategory == 'Care Plans & Files' && (r.isSoapNote || r.isClinicalReport)) return false;

      if (_searchQuery.isNotEmpty) {
        final q = _searchQuery.toLowerCase();
        final matchTitle = r.title.toLowerCase().contains(q);
        final matchSummary = r.summary?.toLowerCase().contains(q) ?? false;
        final matchPhysio = r.sharedByPhysioName.toLowerCase().contains(q);
        final matchType = r.categoryLabel.toLowerCase().contains(q);
        return matchTitle || matchSummary || matchPhysio || matchType;
      }

      return true;
    }).toList();
  }

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
    return const Color(0xFFE65100);
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(sharedReportsProvider);
    final filtered = _filterReports(state.reports);

    return AppPageScaffold(
      title: '${widget.pet.petName}\'s Documents',
      actions: [
        IconButton(
          icon: const Icon(Icons.picture_as_pdf_rounded),
          tooltip: 'Download Full Progress Report',
          onPressed: _downloadPetClinicalReport,
        ),
      ],
      body: RefreshIndicator(
        onRefresh: _refresh,
        color: AppColors.sage,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 16, 20, 40),
          children: [
            // Search Bar
            TextField(
              controller: _searchController,
              onChanged: (val) => setState(() => _searchQuery = val.trim()),
              decoration: InputDecoration(
                hintText: 'Search documents, notes, or doctor...',
                hintStyle: const TextStyle(fontSize: 13, color: AppColors.neutralMuted),
                prefixIcon: const Icon(Icons.search, size: 20, color: AppColors.neutralMuted),
                suffixIcon: _searchQuery.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.close, size: 18),
                        onPressed: () {
                          _searchController.clear();
                          setState(() => _searchQuery = '');
                        },
                      )
                    : null,
                filled: true,
                fillColor: Colors.white,
                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: const BorderSide(color: AppColors.neutralGrey),
                ),
                enabledBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: const BorderSide(color: AppColors.neutralGrey),
                ),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: const BorderSide(color: AppColors.sage, width: 1.5),
                ),
              ),
            ),
            const SizedBox(height: 12),

            // Category Filter Chips
            SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: Row(
                children: _categories.map((cat) {
                  final isSelected = _selectedCategory == cat;
                  return Padding(
                    padding: const EdgeInsets.only(right: 8),
                    child: ChoiceChip(
                      label: Text(cat),
                      selected: isSelected,
                      selectedColor: AppColors.sage,
                      backgroundColor: Colors.white,
                      labelStyle: TextStyle(
                        fontSize: 12,
                        fontWeight: isSelected ? FontWeight.w800 : FontWeight.w600,
                        color: isSelected ? Colors.white : AppColors.navy,
                      ),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10),
                        side: BorderSide(
                          color: isSelected ? AppColors.sage : AppColors.neutralGrey,
                        ),
                      ),
                      onSelected: (_) => setState(() => _selectedCategory = cat),
                    ),
                  );
                }).toList(),
              ),
            ),
            const SizedBox(height: 16),

            // Loading state
            if (state.isLoading)
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 40),
                child: Center(child: CircularProgressIndicator(color: AppColors.sage)),
              )
            // Empty state
            else if (filtered.isEmpty)
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 40),
                child: AppEmptyState(
                  icon: Icons.folder_open_outlined,
                  title: _searchQuery.isNotEmpty
                      ? 'No matching documents'
                      : 'No ${_selectedCategory != 'All' ? _selectedCategory : 'documents'} found',
                  message: _searchQuery.isNotEmpty
                      ? 'Try adjusting your search terms or clearing the filter.'
                      : 'When your physiotherapist shares session notes, progress reports, or care files, they will appear here.',
                  action: (_searchQuery.isNotEmpty || _selectedCategory != 'All')
                      ? OutlinedButton(
                          onPressed: () {
                            _searchController.clear();
                            setState(() {
                              _searchQuery = '';
                              _selectedCategory = 'All';
                            });
                          },
                          child: const Text('Clear Filters'),
                        )
                      : null,
                ),
              )
            // List of Document Cards
            else
              ...filtered.map((report) => _buildReportCard(report)),
          ],
        ),
      ),
    );
  }

  Widget _buildReportCard(SharedReportModel report) {
    final dateStr =
        '${report.sharedAtUtc.year}-${report.sharedAtUtc.month.toString().padLeft(2, '0')}-${report.sharedAtUtc.day.toString().padLeft(2, '0')}';
    final categoryColor = _getCategoryColor(report);
    final categoryIcon = _getCategoryIcon(report);

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: SectionCard(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Top Row: Category Tag, Date & Type Icon
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Flexible(
                  child: Container(
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
                        const SizedBox(width: 5),
                        Flexible(
                          child: Text(
                            report.categoryLabel,
                            overflow: TextOverflow.ellipsis,
                            style: TextStyle(
                              fontSize: 11,
                              fontWeight: FontWeight.w800,
                              color: categoryColor,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                Text(
                  dateStr,
                  style: const TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.w700,
                    color: AppColors.neutralMuted,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 10),

            // Document Title
            Text(
              report.title,
              style: const TextStyle(
                fontWeight: FontWeight.w800,
                fontSize: 15,
                color: AppColors.navy,
                height: 1.3,
              ),
            ),
            const SizedBox(height: 4),

            // Clinician
            Text(
              'Shared by ${report.sharedByPhysioName}',
              style: const TextStyle(fontSize: 12, color: AppColors.neutralMuted),
            ),

            // Clinical Summary snippet
            if (report.summary != null && report.summary!.isNotEmpty) ...[
              const SizedBox(height: 10),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(
                  color: AppColors.surface,
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(color: AppColors.neutralGrey),
                ),
                child: Text(
                  report.summary!,
                  maxLines: 3,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 12,
                    height: 1.45,
                    color: AppColors.neutralDark,
                  ),
                ),
              ),
            ],

            const SizedBox(height: 14),

            // Actions Row with responsive Wrap
            Align(
              alignment: Alignment.centerRight,
              child: Wrap(
                alignment: WrapAlignment.end,
                crossAxisAlignment: WrapCrossAlignment.center,
                spacing: 8,
                runSpacing: 8,
                children: [
                  if (report.isSoapNote) ...[
                    OutlinedButton.icon(
                      style: OutlinedButton.styleFrom(
                        foregroundColor: AppColors.navy,
                        side: const BorderSide(color: AppColors.neutralGrey),
                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                      onPressed: () => _openSoapDetail(report.soapNoteId),
                      icon: const Icon(Icons.visibility_outlined, size: 16),
                      label: const Text(
                        'View Details',
                        style: TextStyle(fontWeight: FontWeight.w700, fontSize: 12),
                      ),
                    ),
                    ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.sage,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                      onPressed: () => _openPdfReport(report.soapNoteId),
                      icon: const Icon(Icons.picture_as_pdf_rounded, size: 16),
                      label: const Text(
                        'Download PDF',
                        style: TextStyle(fontWeight: FontWeight.w700, fontSize: 12),
                      ),
                    ),
                  ] else if (report.isClinicalReport) ...[
                    ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF1E6E8E),
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                      onPressed: _downloadPetClinicalReport,
                      icon: const Icon(Icons.download_rounded, size: 16),
                      label: const Text(
                        'Download Clinical Report',
                        style: TextStyle(fontWeight: FontWeight.w700, fontSize: 12),
                      ),
                    ),
                  ] else ...[
                    ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: categoryColor,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                      onPressed: () {
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(content: Text('Opening document: ${report.title}')),
                        );
                      },
                      icon: const Icon(Icons.file_download_outlined, size: 16),
                      label: const Text(
                        'Open File',
                        style: TextStyle(fontWeight: FontWeight.w700, fontSize: 12),
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
