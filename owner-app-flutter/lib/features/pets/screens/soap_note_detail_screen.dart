import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/pet_avatar.dart';
import '../../../core/widgets/section_card.dart';
import '../models/pet.dart';
import '../models/soap_note_model.dart';
import '../providers/shared_reports_provider.dart';

class SoapNoteDetailScreen extends ConsumerStatefulWidget {
  const SoapNoteDetailScreen({
    super.key,
    required this.pet,
    required this.soapNoteId,
    this.initialSoapNote,
  });

  final Pet pet;
  final int soapNoteId;
  final SoapNoteModel? initialSoapNote;

  @override
  ConsumerState<SoapNoteDetailScreen> createState() => _SoapNoteDetailScreenState();
}

class _SoapNoteDetailScreenState extends ConsumerState<SoapNoteDetailScreen> {
  SoapNoteModel? _soapNote;
  bool _isLoading = true;
  bool _isDownloadingPdf = false;

  @override
  void initState() {
    super.initState();
    _soapNote = widget.initialSoapNote;
    if (_soapNote != null) {
      _isLoading = false;
    }
    _loadSoapNote();
  }

  Future<void> _loadSoapNote() async {
    final note = await ref
        .read(sharedReportsProvider.notifier)
        .fetchSoapNoteById(widget.soapNoteId);
    if (mounted) {
      setState(() {
        _soapNote = note ?? _soapNote;
        _isLoading = false;
      });
    }
  }

  Future<void> _downloadPdf() async {
    setState(() => _isDownloadingPdf = true);
    try {
      final success = await ref
          .read(sharedReportsProvider.notifier)
          .downloadSoapNotePdf(widget.soapNoteId, widget.pet.petName);

      if (!success && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not download PDF report from server.')),
        );
      }
    } catch (err) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('PDF download error: $err')),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isDownloadingPdf = false);
      }
    }
  }

  Color _getScoreColor(int? score, int max) {
    if (score == null) return AppColors.neutralMuted;
    final ratio = score / max;
    if (ratio <= 0.3) return const Color(0xFF2E7D32); // Green
    if (ratio <= 0.6) return const Color(0xFFE65100); // Orange
    return const Color(0xFFC62828); // Red
  }

  @override
  Widget build(BuildContext context) {
    final note = _soapNote;

    return AppPageScaffold(
      title: 'Clinical SOAP Assessment',
      actions: [
        IconButton(
          icon: const Icon(Icons.picture_as_pdf_outlined),
          tooltip: 'Download Official PDF Report',
          onPressed: _downloadPdf,
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
                backgroundColor: AppColors.sage,
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                elevation: 2,
              ),
              onPressed: _isDownloadingPdf ? null : _downloadPdf,
              icon: _isDownloadingPdf
                  ? const SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                    )
                  : const Icon(Icons.picture_as_pdf_rounded, size: 22),
              label: Text(
                _isDownloadingPdf ? 'Opening PDF Report...' : 'Download Official PDF Report',
                style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 15),
              ),
            ),
          ),
        ),
      ),
      body: _isLoading
          ? const Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  CircularProgressIndicator(color: AppColors.sage),
                  SizedBox(height: 16),
                  Text('Loading clinical assessment details...', style: TextStyle(color: AppColors.neutralMuted)),
                ],
              ),
            )
          : note == null
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24.0),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(Icons.assignment_late_outlined, size: 56, color: Colors.grey.shade400),
                        const SizedBox(height: 16),
                        const Text(
                          'Assessment Not Available',
                          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: AppColors.navy),
                        ),
                        const SizedBox(height: 8),
                        const Text(
                          'Could not load the full clinical record details. You can still download the PDF report.',
                          textAlign: TextAlign.center,
                          style: TextStyle(color: AppColors.neutralMuted),
                        ),
                        const SizedBox(height: 20),
                        ElevatedButton.icon(
                          onPressed: _downloadPdf,
                          icon: const Icon(Icons.download_rounded),
                          label: const Text('Download PDF Report'),
                        ),
                      ],
                    ),
                  ),
                )
              : ListView(
                  padding: const EdgeInsets.fromLTRB(20, 16, 20, 120),
                  children: [
                    // Header Card: Patient, Clinician & Date
                    _buildHeaderCard(note),

                    const SizedBox(height: 16),

                    // Key Objective Physical Metrics & Scores Panel
                    _buildScoresPanel(note),

                    const SizedBox(height: 16),

                    // Divided Clinical Sections: S, O, A, P
                    _buildSectionCard(
                      letter: 'S',
                      badgeColor: const Color(0xFF1E6E8E),
                      title: 'Subjective',
                      subtitle: 'Owner Observations & Feedback at Home',
                      content: note.subjective.isNotEmpty
                          ? note.subjective
                          : 'No subjective symptoms or owner notes recorded for this consultation.',
                    ),

                    const SizedBox(height: 12),

                    _buildSectionCard(
                      letter: 'O',
                      badgeColor: const Color(0xFF6B7A4D),
                      title: 'Objective',
                      subtitle: 'Physical Examination & Clinical Measurements',
                      content: note.objective.isNotEmpty
                          ? note.objective
                          : 'No physical examination notes recorded.',
                      customMetrics: note.customMetrics,
                    ),

                    const SizedBox(height: 12),

                    _buildSectionCard(
                      letter: 'A',
                      badgeColor: const Color(0xFF8C5E58),
                      title: 'Action & Treatment',
                      subtitle: 'Therapies, Modalities & In-Clinic Exercises',
                      content: note.action.isNotEmpty
                          ? note.action
                          : 'No in-session treatments recorded.',
                    ),

                    const SizedBox(height: 12),

                    _buildSectionCard(
                      letter: 'P',
                      badgeColor: const Color(0xFF5E548E),
                      title: 'Plan & Home Care',
                      subtitle: 'Prescribed Home Program & Next Session Goals',
                      content: note.plan.isNotEmpty
                          ? note.plan
                          : 'Continue standard maintenance protocol.',
                      isHighlighted: true,
                    ),

                    if (note.rawTranscript != null && note.rawTranscript!.isNotEmpty) ...[
                      const SizedBox(height: 16),
                      _buildTranscriptCard(note),
                    ],
                  ],
                ),
    );
  }

  Widget _buildHeaderCard(SoapNoteModel note) {
    final dateStr =
        '${note.sessionDate.year}-${note.sessionDate.month.toString().padLeft(2, '0')}-${note.sessionDate.day.toString().padLeft(2, '0')}';

    return SectionCard(
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
                  note.physioName,
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

  Widget _buildScoresPanel(SoapNoteModel note) {
    final hasScores = note.painScore != null ||
        note.stiffnessScore != null ||
        note.lamenessScore != null ||
        note.customMetrics.isNotEmpty;

    if (!hasScores) return const SizedBox.shrink();

    return SectionCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Row(
            children: [
              Icon(Icons.analytics_outlined, size: 18, color: AppColors.sage),
              SizedBox(width: 8),
              Text(
                'Clinical Ratings & Examination Scores',
                style: TextStyle(
                  fontWeight: FontWeight.w800,
                  fontSize: 14,
                  color: AppColors.navy,
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          Row(
            children: [
              if (note.painScore != null)
                Expanded(
                  child: _buildScoreBadge(
                    label: 'Pain Level',
                    score: '${note.painScore}/10',
                    color: _getScoreColor(note.painScore, 10),
                    icon: Icons.healing_outlined,
                  ),
                ),
              if (note.painScore != null && note.stiffnessScore != null)
                const SizedBox(width: 8),
              if (note.stiffnessScore != null)
                Expanded(
                  child: _buildScoreBadge(
                    label: 'Stiffness',
                    score: '${note.stiffnessScore}/10',
                    color: _getScoreColor(note.stiffnessScore, 10),
                    icon: Icons.accessibility_new_outlined,
                  ),
                ),
              if (note.lamenessScore != null) ...[
                const SizedBox(width: 8),
                Expanded(
                  child: _buildScoreBadge(
                    label: 'Lameness',
                    score: 'Grade ${note.lamenessScore}/5',
                    color: _getScoreColor(note.lamenessScore, 5),
                    icon: Icons.pets_outlined,
                  ),
                ),
              ],
            ],
          ),
          if (note.customMetrics.isNotEmpty) ...[
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: note.customMetrics.map((m) {
                return Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                  decoration: BoxDecoration(
                    color: AppColors.surface,
                    borderRadius: BorderRadius.circular(10),
                    border: Border.all(color: AppColors.neutralGrey),
                  ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        '${m.name}: ',
                        style: const TextStyle(
                          fontSize: 12,
                          color: AppColors.neutralDark,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      Text(
                        '${m.value.toStringAsFixed(m.value.truncateToDouble() == m.value ? 0 : 1)}${m.unitOrDescriptor != null ? " ${m.unitOrDescriptor}" : ""}',
                        style: const TextStyle(
                          fontSize: 12,
                          fontWeight: FontWeight.w800,
                          color: AppColors.sage,
                        ),
                      ),
                    ],
                  ),
                );
              }).toList(),
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildScoreBadge({
    required String label,
    required String score,
    required Color color,
    required IconData icon,
  }) {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 10, horizontal: 8),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color.withValues(alpha: 0.25)),
      ),
      child: Column(
        children: [
          Icon(icon, size: 18, color: color),
          const SizedBox(height: 4),
          Text(
            score,
            style: TextStyle(
              fontWeight: FontWeight.w800,
              fontSize: 13,
              color: color,
            ),
          ),
          Text(
            label,
            style: TextStyle(
              fontSize: 10,
              fontWeight: FontWeight.w600,
              color: Colors.grey.shade700,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSectionCard({
    required String letter,
    required Color badgeColor,
    required String title,
    required String subtitle,
    required String content,
    List<CustomMetricModel>? customMetrics,
    bool isHighlighted = false,
  }) {
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
                  color: badgeColor,
                  borderRadius: BorderRadius.circular(8),
                ),
                alignment: Alignment.center,
                child: Text(
                  letter,
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.w900,
                    fontSize: 14,
                  ),
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: const TextStyle(
                        fontWeight: FontWeight.w800,
                        fontSize: 15,
                        color: AppColors.navy,
                      ),
                    ),
                    Text(
                      subtitle,
                      style: const TextStyle(
                        fontSize: 11,
                        color: AppColors.neutralMuted,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: isHighlighted
                  ? AppColors.sageMuted.withValues(alpha: 0.3)
                  : AppColors.surface,
              borderRadius: BorderRadius.circular(10),
              border: Border.all(
                color: isHighlighted
                    ? AppColors.sage.withValues(alpha: 0.3)
                    : AppColors.neutralGrey,
              ),
            ),
            child: Text(
              content,
              style: const TextStyle(
                fontSize: 13,
                height: 1.5,
                color: AppColors.neutralDark,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTranscriptCard(SoapNoteModel note) {
    return SectionCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Row(
            children: [
              Icon(Icons.record_voice_over_outlined, size: 18, color: Color(0xFF7B1FA2)),
              SizedBox(width: 8),
              Text(
                'Recorded Consultation Voice Memo',
                style: TextStyle(
                  fontWeight: FontWeight.w800,
                  fontSize: 13,
                  color: Color(0xFF4A148C),
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: const Color(0xFFF3E5F5).withValues(alpha: 0.5),
              borderRadius: BorderRadius.circular(10),
              border: Border.all(color: const Color(0xFFE1BEE7)),
            ),
            child: Text(
              '"${note.rawTranscript}"',
              style: const TextStyle(
                fontSize: 12,
                fontStyle: FontStyle.italic,
                height: 1.4,
                color: Color(0xFF4A148C),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
