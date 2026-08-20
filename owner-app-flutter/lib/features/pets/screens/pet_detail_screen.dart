import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/utils/formatters.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/pet_avatar.dart';
import '../../../core/widgets/progress_ring.dart';
import '../../../core/widgets/section_card.dart';
import '../../../core/widgets/status_badge.dart';
import '../../exercises/providers/exercise_providers.dart';
import '../../exercises/screens/exercise_program_screen.dart';
import '../../shell/main_shell.dart';
import '../../tracking/screens/tracking_screen.dart';
import '../models/pet.dart';
import 'owner_notes_history_screen.dart';
import 'saved_reports_screen.dart';

class PetDetailScreen extends ConsumerStatefulWidget {
  const PetDetailScreen({
    super.key,
    required this.pet,
    this.initialTab = 0,
  });

  final Pet pet;
  final int initialTab;

  @override
  ConsumerState<PetDetailScreen> createState() => _PetDetailScreenState();
}

class _PetDetailScreenState extends ConsumerState<PetDetailScreen>
    with SingleTickerProviderStateMixin {
  late final TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(
      length: 4,
      vsync: this,
      initialIndex: widget.initialTab.clamp(0, 3),
    );
    Future.microtask(
      () => ref.read(rehabProgramsProvider(widget.pet.petId).notifier).loadPrograms(
            widget.pet.petId,
            force: true,
          ),
    );
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  void _messageTherapist() {
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(
        builder: (_) => MainShell(
          initialTab: 2,
          messagesPetId: widget.pet.petId,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final pet = widget.pet;
    final subtitle = formatPetSubtitle(breed: pet.breed, birthDate: pet.birthDate);

    return Scaffold(
      backgroundColor: AppColors.surface,
      appBar: AppBar(
        title: Row(
          children: [
            PetAvatar(name: pet.petName, species: pet.species, size: 36),
            const SizedBox(width: 10),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(pet.petName, style: const TextStyle(fontSize: 17)),
                  if (subtitle.isNotEmpty)
                    Text(
                      subtitle,
                      style: const TextStyle(fontSize: 12, color: AppColors.neutralMuted),
                    ),
                ],
              ),
            ),
          ],
        ),
        bottom: TabBar(
          controller: _tabController,
          isScrollable: true,
          tabAlignment: TabAlignment.start,
          labelColor: AppColors.navy,
          unselectedLabelColor: AppColors.neutralMuted,
          indicatorColor: AppColors.sage,
          indicatorWeight: 3,
          labelStyle: const TextStyle(fontWeight: FontWeight.w700, fontSize: 14),
          tabs: const [
            Tab(text: 'Overview'),
            Tab(text: 'Plan'),
            Tab(text: 'Progress'),
            Tab(text: 'Notes'),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: [
          _OverviewTab(
            pet: pet,
            onMessage: _messageTherapist,
            onOpenPlan: () => _tabController.animateTo(1),
            onOpenNotes: () => _tabController.animateTo(3),
          ),
          _PlanTab(pet: pet),
          _ProgressTab(pet: pet),
          _NotesTab(pet: pet),
        ],
      ),
      bottomNavigationBar: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 8, 20, 12),
          child: ElevatedButton.icon(
            onPressed: _messageTherapist,
            icon: const Icon(Icons.chat_bubble_outline_rounded, size: 20),
            label: const Text('Message Therapist'),
          ),
        ),
      ),
    );
  }
}

class _OverviewTab extends ConsumerWidget {
  const _OverviewTab({
    required this.pet,
    required this.onMessage,
    required this.onOpenPlan,
    required this.onOpenNotes,
  });

  final Pet pet;
  final VoidCallback onMessage;
  final VoidCallback onOpenPlan;
  final VoidCallback onOpenNotes;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final progress = placeholderWeeklyProgress(pet.petId);
    final programState = ref.watch(rehabProgramsProvider(pet.petId));
    final latestNote = pet.medicalHistories.isNotEmpty
        ? pet.medicalHistories.first.clinicianNotes ??
            pet.medicalHistories.first.diagnosis
        : null;

    return PageWashBackground(
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 16, 20, 100),
        children: [
          SectionCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    PetAvatar(name: pet.petName, species: pet.species, size: 56),
                    const SizedBox(width: 14),
                    Expanded(
                      child: Text(
                        'Helping every pet move better.',
                        style: GoogleFonts.caveat(
                          fontSize: 20,
                          fontWeight: FontWeight.w600,
                          color: AppColors.sage,
                        ),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          SectionCard(
            child: Row(
              children: [
                ProgressRing(percent: progress, label: 'Weekly Goal'),
                const SizedBox(width: 20),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      _MetricRow(
                        label: 'Exercises',
                        value: programState.activeProgram != null ? 'Active plan' : 'No plan yet',
                      ),
                      const SizedBox(height: 8),
                      const _MetricRow(label: 'Pain Level', value: 'Low'),
                      const SizedBox(height: 8),
                      const _MetricRow(label: 'Mobility', value: 'Improving'),
                      const SizedBox(height: 8),
                      const _MetricRow(label: 'Energy', value: 'Good'),
                    ],
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          SectionCard(
            onTap: () => Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => OwnerNotesHistoryScreen(pet: pet)),
            ),
            child: Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(10),
                  decoration: BoxDecoration(
                    color: AppColors.sage.withValues(alpha: 0.12),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(Icons.rate_review_outlined, color: AppColors.sage),
                ),
                const SizedBox(width: 14),
                const Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Home Observations & Notes',
                        style: TextStyle(fontWeight: FontWeight.w700, color: AppColors.navy, fontSize: 14),
                      ),
                      Text(
                        'View submitted history & share stiffness or pain updates with your physio.',
                        style: TextStyle(color: AppColors.neutralMuted, fontSize: 12),
                      ),
                    ],
                  ),
                ),
                Icon(Icons.chevron_right_rounded, color: AppColors.navy.withValues(alpha: 0.3)),
              ],
            ),
          ),
          const SizedBox(height: 16),
          GridView.count(
            crossAxisCount: 2,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            mainAxisSpacing: 10,
            crossAxisSpacing: 10,
            childAspectRatio: 1.6,
            children: [
              _QuickAction(
                icon: Icons.assignment_outlined,
                label: 'Treatment Plan',
                onTap: onOpenPlan,
              ),
              _QuickAction(
                icon: Icons.fitness_center_outlined,
                label: 'Exercises',
                onTap: () => Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (_) => ExerciseProgramScreen(pet: pet),
                  ),
                ),
              ),
              _QuickAction(
                icon: Icons.folder_shared_outlined,
                label: 'Documents',
                onTap: () => Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => SavedReportsScreen(pet: pet)),
                ),
              ),
              _QuickAction(
                icon: Icons.note_alt_outlined,
                label: 'Notes',
                onTap: () => Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => OwnerNotesHistoryScreen(pet: pet)),
                ),
              ),
            ],
          ),
          if (latestNote != null) ...[
            const SizedBox(height: 16),
            const Text(
              'Latest Update',
              style: TextStyle(
                fontWeight: FontWeight.w700,
                color: AppColors.navy,
                fontSize: 15,
              ),
            ),
            const SizedBox(height: 8),
            SectionCard(
              child: Text(
                latestNote,
                style: const TextStyle(color: AppColors.neutralDark, height: 1.45),
              ),
            ),
          ],
        ],
      ),
    );
  }
}

class _MetricRow extends StatelessWidget {
  const _MetricRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: Text(label, style: const TextStyle(color: AppColors.neutralMuted, fontSize: 13)),
        ),
        Text(value, style: const TextStyle(fontWeight: FontWeight.w700, color: AppColors.navy)),
      ],
    );
  }
}

class _QuickAction extends StatelessWidget {
  const _QuickAction({
    required this.icon,
    required this.label,
    required this.onTap,
  });

  final IconData icon;
  final String label;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      onTap: onTap,
      padding: const EdgeInsets.all(14),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, color: AppColors.sage),
          const SizedBox(height: 8),
          Text(
            label,
            textAlign: TextAlign.center,
            style: const TextStyle(
              fontWeight: FontWeight.w600,
              color: AppColors.navy,
              fontSize: 13,
            ),
          ),
        ],
      ),
    );
  }
}

class _PlanTab extends ConsumerWidget {
  const _PlanTab({required this.pet});

  final Pet pet;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final programState = ref.watch(rehabProgramsProvider(pet.petId));
    final program = programState.activeProgram;

    if (programState.isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (program == null) {
      return const Padding(
        padding: EdgeInsets.all(24),
        child: AppEmptyState(
          icon: Icons.assignment_outlined,
          title: 'No treatment plan yet',
          message: 'Your physiotherapist will publish a plan here when ready.',
        ),
      );
    }

    return PageWashBackground(
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 16, 20, 100),
        children: [
          SectionCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Expanded(
                      child: Text(
                        program.programTitle,
                        style: const TextStyle(
                          fontWeight: FontWeight.w800,
                          color: AppColors.navy,
                          fontSize: 16,
                        ),
                      ),
                    ),
                    const StatusBadge(label: 'In progress'),
                  ],
                ),
                const SizedBox(height: 8),
                Text(
                  '${formatAppointmentDate(DateTime.parse(program.startDate))}'
                  '${program.endDate != null ? ' – ${formatAppointmentDate(DateTime.parse(program.endDate!))}' : ''}',
                  style: const TextStyle(color: AppColors.neutralMuted, fontSize: 13),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          if (program.notes != null && program.notes!.isNotEmpty) ...[
            const Text(
              'Goals',
              style: TextStyle(fontWeight: FontWeight.w700, color: AppColors.navy),
            ),
            const SizedBox(height: 8),
            SectionCard(
              child: Text(program.notes!, style: const TextStyle(height: 1.5)),
            ),
            const SizedBox(height: 16),
          ],
          const Text(
            'This Week',
            style: TextStyle(fontWeight: FontWeight.w700, color: AppColors.navy),
          ),
          const SizedBox(height: 8),
          SectionCard(
            child: Column(
              children: [
                LinearProgressIndicator(
                  value: 0.5,
                  backgroundColor: AppColors.neutralGrey,
                  color: AppColors.sage,
                  minHeight: 6,
                  borderRadius: BorderRadius.circular(3),
                ),
                const SizedBox(height: 8),
                const Align(
                  alignment: Alignment.centerLeft,
                  child: Text('Week 1 of 2', style: TextStyle(color: AppColors.neutralMuted)),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          _PlanLinkTile(
            icon: Icons.fitness_center_outlined,
            title: 'Exercises',
            subtitle: '${program.exercises.length} prescribed',
            onTap: () => Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => ExerciseProgramScreen(pet: pet)),
            ),
          ),
          _PlanLinkTile(
            icon: Icons.home_outlined,
            title: 'At-Home Care',
            subtitle: 'Daily care instructions',
            onTap: () {},
          ),
          _PlanLinkTile(
            icon: Icons.medical_information_outlined,
            title: 'Notes from your Physio',
            subtitle: program.notes ?? 'No notes yet',
            onTap: () {},
          ),
        ],
      ),
    );
  }
}

class _PlanLinkTile extends StatelessWidget {
  const _PlanLinkTile({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: SectionCard(
        onTap: onTap,
        child: Row(
          children: [
            Icon(icon, color: AppColors.sage),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(title, style: const TextStyle(fontWeight: FontWeight.w700, color: AppColors.navy)),
                  Text(subtitle, style: const TextStyle(color: AppColors.neutralMuted, fontSize: 13)),
                ],
              ),
            ),
            Icon(Icons.chevron_right_rounded, color: AppColors.navy.withValues(alpha: 0.3)),
          ],
        ),
      ),
    );
  }
}

class _ProgressTab extends StatelessWidget {
  const _ProgressTab({required this.pet});

  final Pet pet;

  @override
  Widget build(BuildContext context) {
    final progress = placeholderWeeklyProgress(pet.petId);

    return PageWashBackground(
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 16, 20, 100),
        children: [
          SectionCard(
            child: Column(
              children: [
                ProgressRing(percent: progress, size: 120, label: 'Weekly Goal'),
                const SizedBox(height: 16),
                const Text(
                  'Track daily pain, energy, and mobility to see trends over time.',
                  textAlign: TextAlign.center,
                  style: TextStyle(color: AppColors.neutralMuted, height: 1.45),
                ),
                const SizedBox(height: 16),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: () => Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) => TrackingScreen(petId: pet.petId, petName: pet.petName),
                      ),
                    ),
                    child: const Text('Open Daily Tracking'),
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

class _NotesTab extends StatelessWidget {
  const _NotesTab({required this.pet});

  final Pet pet;

  @override
  Widget build(BuildContext context) {
    if (pet.medicalHistories.isEmpty) {
      return const Padding(
        padding: EdgeInsets.all(24),
        child: AppEmptyState(
          icon: Icons.note_alt_outlined,
          title: 'No notes yet',
          message: 'Medical history and clinician notes will appear here.',
        ),
      );
    }

    return PageWashBackground(
      child: ListView.separated(
        padding: const EdgeInsets.fromLTRB(20, 16, 20, 100),
        itemCount: pet.medicalHistories.length,
        separatorBuilder: (_, _) => const SizedBox(height: 10),
        itemBuilder: (context, index) {
          final history = pet.medicalHistories[index];
          return SectionCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  history.diagnosis,
                  style: const TextStyle(fontWeight: FontWeight.w800, color: AppColors.navy),
                ),
                if (history.injuryOrCondition != null) ...[
                  const SizedBox(height: 6),
                  Text(history.injuryOrCondition!, style: const TextStyle(color: AppColors.neutralMuted)),
                ],
                if (history.clinicianNotes != null) ...[
                  const SizedBox(height: 10),
                  Text(history.clinicianNotes!, style: const TextStyle(height: 1.45)),
                ],
              ],
            ),
          );
        },
      ),
    );
  }
}
