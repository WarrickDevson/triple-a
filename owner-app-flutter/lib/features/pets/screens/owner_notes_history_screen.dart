import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/section_card.dart';
import '../models/pet.dart';
import '../providers/owner_notes_provider.dart';
import 'submit_owner_note_dialog.dart';

class OwnerNotesHistoryScreen extends ConsumerWidget {
  const OwnerNotesHistoryScreen({super.key, required this.pet});

  final Pet pet;

  String _formatDate(DateTime dt) {
    final local = dt.toLocal();
    final monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    final m = monthNames[local.month - 1];
    final h = local.hour > 12 ? local.hour - 12 : (local.hour == 0 ? 12 : local.hour);
    final ampm = local.hour >= 12 ? 'PM' : 'AM';
    final min = local.minute.toString().padLeft(2, '0');
    return '$m ${local.day}, ${local.year} · $h:$min $ampm';
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final notesAsync = ref.watch(ownerNotesListProvider(pet.petId));

    return Scaffold(
      appBar: AppBar(
        title: Text('${pet.petName}\'s Home Notes History'),
        backgroundColor: Colors.white,
        foregroundColor: AppColors.navy,
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.add_comment_outlined, color: AppColors.sage),
            tooltip: 'Add Note',
            onPressed: () => showDialog(
              context: context,
              builder: (_) => SubmitOwnerNoteDialog(pet: pet),
            ),
          ),
        ],
      ),
      body: notesAsync.when(
        data: (notes) {
          if (notes.isEmpty) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.notes_outlined, size: 64, color: AppColors.neutralMuted.withValues(alpha: 0.5)),
                    const SizedBox(height: 16),
                    const Text(
                      'No Home Observations Submitted Yet',
                      style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: AppColors.navy),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'Share notes about ${pet.petName}\'s stiffness, pain, or exercise updates so your physio can review them.',
                      textAlign: TextAlign.center,
                      style: const TextStyle(fontSize: 13, color: AppColors.neutralMuted, height: 1.4),
                    ),
                    const SizedBox(height: 24),
                    ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.sage,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      onPressed: () => showDialog(
                        context: context,
                        builder: (_) => SubmitOwnerNoteDialog(pet: pet),
                      ),
                      icon: const Icon(Icons.add, size: 18),
                      label: const Text('Submit First Observation'),
                    ),
                  ],
                ),
              ),
            );
          }

          return RefreshIndicator(
            onRefresh: () async => ref.refresh(ownerNotesListProvider(pet.petId)),
            child: ListView.separated(
              padding: const EdgeInsets.all(16),
              itemCount: notes.length,
              separatorBuilder: (context, index) => const SizedBox(height: 12),
              itemBuilder: (context, index) {
                final note = notes[index];
                final dateStr = _formatDate(note.noteDate);

                return SectionCard(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Row(
                            children: [
                              Container(
                                padding: const EdgeInsets.all(6),
                                decoration: BoxDecoration(
                                  color: AppColors.sage.withValues(alpha: 0.12),
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                child: const Icon(Icons.rate_review_outlined, size: 16, color: AppColors.sage),
                              ),
                              const SizedBox(width: 8),
                              Text(
                                dateStr,
                                style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: AppColors.neutralMuted),
                              ),
                            ],
                          ),
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                            decoration: BoxDecoration(
                              color: note.isReviewed
                                  ? Colors.green.shade50
                                  : Colors.amber.shade50,
                              borderRadius: BorderRadius.circular(12),
                              border: Border.all(
                                color: note.isReviewed
                                    ? Colors.green.shade200
                                    : Colors.amber.shade200,
                              ),
                            ),
                            child: Text(
                              note.isReviewed ? 'Reviewed by Physio' : 'Pending Review',
                              style: TextStyle(
                                fontSize: 10,
                                fontWeight: FontWeight.bold,
                                color: note.isReviewed
                                    ? Colors.green.shade700
                                    : Colors.amber.shade800,
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),
                      Text(
                        '"${note.notes}"',
                        style: const TextStyle(fontSize: 14, color: AppColors.navy, height: 1.45, fontStyle: FontStyle.italic),
                      ),
                      if (note.painObserved != null || note.energyObserved != null) ...[
                        const SizedBox(height: 12),
                        Wrap(
                          spacing: 8,
                          children: [
                            if (note.painObserved != null)
                              Chip(
                                labelPadding: const EdgeInsets.symmetric(horizontal: 4),
                                visualDensity: VisualDensity.compact,
                                backgroundColor: Colors.grey.shade100,
                                label: Text(
                                  'Pain: ${note.painObserved}/10',
                                  style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppColors.navy),
                                ),
                              ),
                            if (note.energyObserved != null)
                              Chip(
                                labelPadding: const EdgeInsets.symmetric(horizontal: 4),
                                visualDensity: VisualDensity.compact,
                                backgroundColor: Colors.grey.shade100,
                                label: Text(
                                  'Energy: ${note.energyObserved}/10',
                                  style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppColors.navy),
                                ),
                              ),
                          ],
                        ),
                      ],
                    ],
                  ),
                );
              },
            ),
          );
        },
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (err, stack) => Center(
          child: Text('Error loading notes: $err'),
        ),
      ),
    );
  }
}
