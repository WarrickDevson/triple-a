import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/section_card.dart';
import '../models/pet.dart';
import '../models/shared_report_model.dart';
import '../providers/pets_provider.dart';
import '../providers/shared_reports_provider.dart';
import 'soap_note_detail_screen.dart';

class SavedReportsScreen extends ConsumerStatefulWidget {
  const SavedReportsScreen({super.key, this.pet});

  final Pet? pet;

  @override
  ConsumerState<SavedReportsScreen> createState() => _SavedReportsScreenState();
}

class _SavedReportsScreenState extends ConsumerState<SavedReportsScreen> {
  int? _selectedPetId;
  String? _selectedPetName;

  String _selectedCategory = 'All';
  String _selectedDateRange = 'All Time';
  bool _sortNewestFirst = true;
  String _searchQuery = '';
  final TextEditingController _searchController = TextEditingController();

  final List<String> _categories = [
    'All',
    'SOAP Notes',
    'Clinical Reports',
    'Care Plans',
    'Referrals & Files',
  ];

  final List<String> _dateRanges = [
    'All Time',
    'Past 30 Days',
    'Past 90 Days',
    'This Year',
  ];

  @override
  void initState() {
    super.initState();
    if (widget.pet != null) {
      _selectedPetId = widget.pet!.petId;
      _selectedPetName = widget.pet!.petName;
    }
    Future.microtask(() {
      _loadReports();
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _loadReports() async {
    await ref.read(sharedReportsProvider.notifier).fetchSharedReports(_selectedPetId);
  }

  Future<void> _refresh() async {
    await _loadReports();
  }

  void _onPetSelected(int? petId, String? petName) {
    if (_selectedPetId == petId) return;
    setState(() {
      _selectedPetId = petId;
      _selectedPetName = petName;
    });
    ref.read(sharedReportsProvider.notifier).fetchSharedReports(petId);
  }

  void _openPdfReport(int? soapNoteId, String petName) async {
    if (soapNoteId == null) return;
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Downloading SOAP Note PDF...'), duration: Duration(seconds: 1)),
    );
    final success = await ref
        .read(sharedReportsProvider.notifier)
        .downloadSoapNotePdf(soapNoteId, petName);

    if (!success && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not download PDF report from server.')),
      );
    }
  }

  void _downloadPetClinicalReport([int? petId, String? petName]) async {
    final targetPetId = petId ?? _selectedPetId ?? widget.pet?.petId;
    final targetPetName = petName ?? _selectedPetName ?? widget.pet?.petName ?? 'Companion';

    if (targetPetId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Select a companion to download full clinical report.')),
      );
      return;
    }

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Downloading Clinical Progress Report for $targetPetName...'), duration: const Duration(seconds: 1)),
    );
    final success = await ref
        .read(sharedReportsProvider.notifier)
        .downloadPetClinicalReport(targetPetId, targetPetName);

    if (!success && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not download clinical progress report from server.')),
      );
    }
  }

  void _downloadSharedReport(SharedReportModel report) async {
    final petName = report.petName ?? _selectedPetName ?? widget.pet?.petName ?? 'Companion';
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Downloading ${report.title}...'), duration: const Duration(seconds: 1)),
    );
    final success = await ref
        .read(sharedReportsProvider.notifier)
        .downloadSharedReport(report.sharedReportId, petName, report.title);

    if (!success && mounted) {
      _downloadPetClinicalReport(report.petId, petName);
    }
  }

  void _openSoapDetail(SharedReportModel report) {
    if (report.soapNoteId == null) return;
    final pets = ref.read(petsProvider).pets;
    final targetPet = widget.pet ??
        (pets.any((p) => p.petId == report.petId)
            ? pets.firstWhere((p) => p.petId == report.petId)
            : (pets.isNotEmpty
                ? pets.first
                : Pet(
                    petId: report.petId,
                    ownerId: 0,
                    ownerName: 'Owner',
                    petName: report.petName ?? 'Companion',
                    species: 'Canine',
                    medicalHistories: const [],
                  )));

    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => SoapNoteDetailScreen(
          pet: targetPet,
          soapNoteId: report.soapNoteId!,
        ),
      ),
    );
  }

  List<SharedReportModel> _filterReports(List<SharedReportModel> reports) {
    final now = DateTime.now();

    final filtered = reports.where((r) {
      // 1. Companion filter (if not "All Pets")
      if (_selectedPetId != null && r.petId != _selectedPetId) return false;

      // 2. Document Type Category
      if (_selectedCategory == 'SOAP Notes' && !r.isSoapNote) return false;
      if (_selectedCategory == 'Clinical Reports' && !r.isClinicalReport) return false;
      if (_selectedCategory == 'Care Plans' && !r.isHomeProgram) return false;
      if (_selectedCategory == 'Referrals & Files') {
        final upper = r.reportType.toUpperCase();
        if (!upper.contains('REFERRAL') && !upper.contains('IMAGING') && !upper.contains('CONSENT') && !upper.contains('FILE')) {
          return false;
        }
      }

      // 3. Date Range Filter
      if (_selectedDateRange == 'Past 30 Days') {
        final thirtyDaysAgo = now.subtract(const Duration(days: 30));
        if (r.sharedAtUtc.isBefore(thirtyDaysAgo)) return false;
      } else if (_selectedDateRange == 'Past 90 Days') {
        final ninetyDaysAgo = now.subtract(const Duration(days: 90));
        if (r.sharedAtUtc.isBefore(ninetyDaysAgo)) return false;
      } else if (_selectedDateRange == 'This Year') {
        if (r.sharedAtUtc.year != now.year) return false;
      }

      // 4. Search Query
      if (_searchQuery.isNotEmpty) {
        final q = _searchQuery.toLowerCase();
        final matchTitle = r.title.toLowerCase().contains(q);
        final matchSummary = r.summary?.toLowerCase().contains(q) ?? false;
        final matchPhysio = r.sharedByPhysioName.toLowerCase().contains(q);
        final matchType = r.categoryLabel.toLowerCase().contains(q);
        final matchPet = r.petName?.toLowerCase().contains(q) ?? false;
        return matchTitle || matchSummary || matchPhysio || matchType || matchPet;
      }

      return true;
    }).toList();

    // Sort by Date
    filtered.sort((a, b) {
      final comp = b.sharedAtUtc.compareTo(a.sharedAtUtc);
      return _sortNewestFirst ? comp : -comp;
    });

    return filtered;
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
    final pets = ref.watch(petsProvider).pets;
    final filtered = _filterReports(state.reports);

    final title = _selectedPetName != null
        ? '$_selectedPetName\'s Documents'
        : 'Clinical Documents & Reports';

    return AppPageScaffold(
      title: title,
      actions: [
        if (_selectedPetId != null)
          IconButton(
            icon: const Icon(Icons.picture_as_pdf_rounded),
            tooltip: 'Download Full Progress Report',
            onPressed: () => _downloadPetClinicalReport(_selectedPetId, _selectedPetName),
          ),
      ],
      body: RefreshIndicator(
        onRefresh: _refresh,
        color: AppColors.sage,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 16, 20, 40),
          children: [
            // -----------------------------------------------------------------
            // 1. COMPANION SELECTOR CHIPS (ALL PETS + EACH REGISTERED PET)
            // -----------------------------------------------------------------
            if (pets.length > 1 || _selectedPetId != null) ...[
              Padding(
                padding: const EdgeInsets.only(left: 4, bottom: 8),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      'COMPANION FILTER',
                      style: TextStyle(
                        fontSize: 11,
                        fontWeight: FontWeight.w800,
                        letterSpacing: 0.8,
                        color: AppColors.neutralDark.withValues(alpha: 0.6),
                      ),
                    ),
                    if (_selectedPetId != null)
                      GestureDetector(
                        onTap: () => _onPetSelected(null, null),
                        child: const Text(
                          'Show All Companions',
                          style: TextStyle(
                            fontSize: 11.5,
                            fontWeight: FontWeight.w700,
                            color: AppColors.sage,
                          ),
                        ),
                      ),
                  ],
                ),
              ),
              SizedBox(
                height: 40,
                child: ListView(
                  scrollDirection: Axis.horizontal,
                  children: [
                    // "All Pets" Chip
                    Padding(
                      padding: const EdgeInsets.only(right: 8),
                      child: ChoiceChip(
                        avatar: Icon(
                          Icons.dashboard_outlined,
                          size: 15,
                          color: _selectedPetId == null ? Colors.white : AppColors.primaryDark,
                        ),
                        label: const Text('All Pets'),
                        selected: _selectedPetId == null,
                        selectedColor: AppColors.primaryDark,
                        backgroundColor: Colors.white,
                        labelStyle: TextStyle(
                          fontSize: 12,
                          fontWeight: FontWeight.w700,
                          color: _selectedPetId == null ? Colors.white : AppColors.neutralDark,
                        ),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(20),
                          side: BorderSide(
                            color: _selectedPetId == null ? AppColors.primaryDark : AppColors.neutralGrey,
                            width: 1.2,
                          ),
                        ),
                        onSelected: (_) => _onPetSelected(null, null),
                      ),
                    ),
                    // Individual Pet Chips
                    ...pets.map((p) {
                      final isSelected = p.petId == _selectedPetId;
                      return Padding(
                        padding: const EdgeInsets.only(right: 8),
                        child: ChoiceChip(
                          avatar: Icon(
                            Icons.pets,
                            size: 14,
                            color: isSelected ? Colors.white : AppColors.sage,
                          ),
                          label: Text(p.petName),
                          selected: isSelected,
                          selectedColor: AppColors.sage,
                          backgroundColor: Colors.white,
                          labelStyle: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.w700,
                            color: isSelected ? Colors.white : AppColors.neutralDark,
                          ),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(20),
                            side: BorderSide(
                              color: isSelected ? AppColors.sage : AppColors.neutralGrey,
                              width: 1.2,
                            ),
                          ),
                          onSelected: (_) => _onPetSelected(p.petId, p.petName),
                        ),
                      );
                    }),
                  ],
                ),
              ),
              const SizedBox(height: 14),
            ],

            // -----------------------------------------------------------------
            // 2. SEARCH BAR
            // -----------------------------------------------------------------
            TextField(
              controller: _searchController,
              onChanged: (val) => setState(() => _searchQuery = val.trim()),
              decoration: InputDecoration(
                hintText: 'Search title, doctor, note, or pet...',
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

            // -----------------------------------------------------------------
            // 3. DOCUMENT TYPE CATEGORY CHIPS
            // -----------------------------------------------------------------
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
            const SizedBox(height: 10),

            // -----------------------------------------------------------------
            // 4. DATE RANGE & SORT BAR
            // -----------------------------------------------------------------
            Row(
              children: [
                Expanded(
                  child: SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    child: Row(
                      children: _dateRanges.map((range) {
                        final isSelected = _selectedDateRange == range;
                        return Padding(
                          padding: const EdgeInsets.only(right: 6),
                          child: InkWell(
                            onTap: () => setState(() => _selectedDateRange = range),
                            borderRadius: BorderRadius.circular(8),
                            child: Container(
                              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                              decoration: BoxDecoration(
                                color: isSelected
                                    ? AppColors.navy
                                    : AppColors.surface,
                                borderRadius: BorderRadius.circular(8),
                                border: Border.all(
                                  color: isSelected ? AppColors.navy : AppColors.neutralGrey,
                                ),
                              ),
                              child: Text(
                                range,
                                style: TextStyle(
                                  fontSize: 11,
                                  fontWeight: isSelected ? FontWeight.w700 : FontWeight.w600,
                                  color: isSelected ? Colors.white : AppColors.neutralMuted,
                                ),
                              ),
                            ),
                          ),
                        );
                      }).toList(),
                    ),
                  ),
                ),
                // Sort Toggle Button
                InkWell(
                  onTap: () => setState(() => _sortNewestFirst = !_sortNewestFirst),
                  borderRadius: BorderRadius.circular(8),
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 5),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(8),
                      border: Border.all(color: AppColors.neutralGrey),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(
                          _sortNewestFirst ? Icons.arrow_downward : Icons.arrow_upward,
                          size: 13,
                          color: AppColors.navy,
                        ),
                        const SizedBox(width: 4),
                        Text(
                          _sortNewestFirst ? 'Newest' : 'Oldest',
                          style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w700, color: AppColors.navy),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),

            // -----------------------------------------------------------------
            // 5. DOCUMENTS LIST / LOADING / EMPTY STATE
            // -----------------------------------------------------------------
            if (state.isLoading)
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 40),
                child: Center(child: CircularProgressIndicator(color: AppColors.sage)),
              )
            else if (filtered.isEmpty)
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 40),
                child: AppEmptyState(
                  icon: Icons.folder_open_outlined,
                  title: _searchQuery.isNotEmpty
                      ? 'No matching documents'
                      : (_selectedPetName != null
                          ? 'No documents found for $_selectedPetName'
                          : 'No ${_selectedCategory != 'All' ? _selectedCategory : 'clinical documents'} found'),
                  message: _searchQuery.isNotEmpty || _selectedCategory != 'All' || _selectedDateRange != 'All Time'
                      ? 'Try clearing your search or filters to see all records.'
                      : (_selectedPetName != null
                          ? '$_selectedPetName does not have any clinical notes or reports yet. Documents published by your veterinary practice will appear here.'
                          : 'When your physiotherapist shares session notes, progress reports, or care files, they will appear here.'),
                  action: (_searchQuery.isNotEmpty || _selectedCategory != 'All' || _selectedDateRange != 'All Time')
                      ? OutlinedButton(
                          onPressed: () {
                            _searchController.clear();
                            setState(() {
                              _searchQuery = '';
                              _selectedCategory = 'All';
                              _selectedDateRange = 'All Time';
                            });
                          },
                          child: const Text('Clear Filters'),
                        )
                      : null,
                ),
              )
            else ...[
              Padding(
                padding: const EdgeInsets.only(left: 4, bottom: 10),
                child: Text(
                  'SHOWING ${filtered.length} ${filtered.length == 1 ? 'DOCUMENT' : 'DOCUMENTS'}',
                  style: TextStyle(
                    fontSize: 11,
                    fontWeight: FontWeight.w800,
                    letterSpacing: 0.8,
                    color: AppColors.neutralDark.withValues(alpha: 0.55),
                  ),
                ),
              ),
              ...filtered.map((report) => _buildReportCard(report)),
            ],
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
            // Top Row: Category Tag, Companion Name (if viewing all), and Date
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Wrap(
                    spacing: 6,
                    runSpacing: 4,
                    crossAxisAlignment: WrapCrossAlignment.center,
                    children: [
                      // Category Badge
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 3.5),
                        decoration: BoxDecoration(
                          color: categoryColor.withValues(alpha: 0.12),
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: categoryColor.withValues(alpha: 0.3)),
                        ),
                        child: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(categoryIcon, size: 13, color: categoryColor),
                            const SizedBox(width: 5),
                            Text(
                              report.categoryLabel,
                              style: TextStyle(
                                fontSize: 11,
                                fontWeight: FontWeight.w800,
                                color: categoryColor,
                              ),
                            ),
                          ],
                        ),
                      ),
                      // Pet Name Badge (shown if viewing all pets)
                      if (_selectedPetId == null && report.petName != null)
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3.5),
                          decoration: BoxDecoration(
                            color: AppColors.surface,
                            borderRadius: BorderRadius.circular(8),
                            border: Border.all(color: AppColors.neutralGrey),
                          ),
                          child: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              const Icon(Icons.pets, size: 11, color: AppColors.primaryDark),
                              const SizedBox(width: 4),
                              Text(
                                report.petName!,
                                style: const TextStyle(
                                  fontSize: 11,
                                  fontWeight: FontWeight.w700,
                                  color: AppColors.primaryDark,
                                ),
                              ),
                            ],
                          ),
                        ),
                    ],
                  ),
                ),
                const SizedBox(width: 8),
                Padding(
                  padding: const EdgeInsets.only(top: 2),
                  child: Text(
                    dateStr,
                    style: const TextStyle(
                      fontSize: 11.5,
                      fontWeight: FontWeight.w700,
                      color: AppColors.neutralMuted,
                    ),
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

            // Actions Row
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
                      onPressed: () => _openSoapDetail(report),
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
                      onPressed: () => _openPdfReport(report.soapNoteId, report.petName ?? _selectedPetName ?? 'Companion'),
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
                      onPressed: () => _downloadSharedReport(report),
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
                      onPressed: () => _downloadSharedReport(report),
                      icon: const Icon(Icons.file_download_outlined, size: 16),
                      label: const Text(
                        'Download PDF',
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
