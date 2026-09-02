import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../models/owner_subjective_note.dart';
import '../models/pet.dart';
import '../providers/owner_notes_provider.dart';

class SubmitOwnerNoteDialog extends ConsumerStatefulWidget {
  const SubmitOwnerNoteDialog({
    super.key,
    required this.pet,
    this.existingNote,
  });

  final Pet pet;
  final OwnerSubjectiveNote? existingNote;

  @override
  ConsumerState<SubmitOwnerNoteDialog> createState() => _SubmitOwnerNoteDialogState();
}

class _SubmitOwnerNoteDialogState extends ConsumerState<SubmitOwnerNoteDialog> {
  final _notesController = TextEditingController();
  int _painScore = 2;
  int _energyScore = 7;
  bool _submitting = false;

  @override
  void initState() {
    super.initState();
    if (widget.existingNote != null) {
      _notesController.text = widget.existingNote!.notes;
      _painScore = widget.existingNote!.painObserved ?? 2;
      _energyScore = widget.existingNote!.energyObserved ?? 7;
    }
  }

  @override
  void dispose() {
    _notesController.dispose();
    super.dispose();
  }

  String _getPainDescriptor(int score) {
    if (score == 0) return 'None (Comfortable)';
    if (score <= 3) return 'Mild Discomfort';
    if (score <= 6) return 'Moderate Pain';
    if (score <= 8) return 'Noticeable Pain';
    return 'Severe Pain';
  }

  String _getEnergyDescriptor(int score) {
    if (score <= 2) return 'Lethargic / Very Low';
    if (score <= 4) return 'Quiet / Low Energy';
    if (score <= 6) return 'Moderate / Normal';
    if (score <= 8) return 'Good / Playful';
    return 'Very High / Energetic';
  }

  Color _getPainColor(int score) {
    if (score <= 2) return const Color(0xFF6B7A4D);
    if (score <= 5) return Colors.amber.shade700;
    return AppColors.alertRed;
  }

  Color _getEnergyColor(int score) {
    if (score <= 3) return Colors.amber.shade800;
    if (score <= 6) return const Color(0xFF0C3C54);
    return const Color(0xFF6B7A4D);
  }

  Future<void> _submit() async {
    final text = _notesController.text.trim();
    if (text.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please enter your observations or notes.')),
      );
      return;
    }

    setState(() => _submitting = true);

    final bool ok;
    if (widget.existingNote != null) {
      ok = await ref.read(ownerNotesProvider.notifier).updateOwnerSubjectiveNote(
            noteId: widget.existingNote!.ownerSubjectiveNoteId,
            notes: text,
            painObserved: _painScore,
            energyObserved: _energyScore,
          );
    } else {
      ok = await ref.read(ownerNotesProvider.notifier).submitOwnerSubjectiveNote(
            petId: widget.pet.petId,
            notes: text,
            painObserved: _painScore,
            energyObserved: _energyScore,
          );
    }

    setState(() => _submitting = false);

    if (ok && mounted) {
      ref.invalidate(ownerNotesListProvider(widget.pet.petId));
      Navigator.of(context).pop();
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            widget.existingNote != null
                ? 'Your note was updated successfully!'
                : 'Your notes were submitted for your physio\'s review!',
          ),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      title: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: const Color(0xFF6B7A4D).withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(10),
            ),
            child: const Icon(Icons.rate_review_outlined, color: Color(0xFF6B7A4D), size: 22),
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              widget.existingNote != null ? 'Edit Note for Physio' : 'Share Notes with Physio',
              style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: Color(0xFF0C3C54)),
            ),
          ),
        ],
      ),
      content: SingleChildScrollView(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Describe what you observed at home for ${widget.pet.petName} (e.g. stiffness after walks, appetite, new behaviors). Your physio will review this during your session.',
              style: TextStyle(fontSize: 13, color: Colors.grey.shade700, height: 1.4),
            ),
            const SizedBox(height: 14),
            TextField(
              controller: _notesController,
              maxLines: 4,
              decoration: InputDecoration(
                hintText: 'e.g. ${widget.pet.petName} seemed a little stiff on the right hind leg after morning walk, but improved after resting...',
                hintStyle: TextStyle(fontSize: 12, color: Colors.grey.shade400),
                filled: true,
                fillColor: Colors.grey.shade100,
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: BorderSide(color: Colors.grey.shade300),
                ),
                enabledBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: BorderSide(color: Colors.grey.shade200),
                ),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: const BorderSide(color: Color(0xFF6B7A4D), width: 1.5),
                ),
              ),
            ),
            const SizedBox(height: 18),

            // Observed Pain Slider
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.grey.shade50,
                borderRadius: BorderRadius.circular(14),
                border: Border.all(color: Colors.grey.shade200),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Observed Pain',
                        style: TextStyle(fontSize: 13, fontWeight: FontWeight.bold, color: Color(0xFF0C3C54)),
                      ),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                        decoration: BoxDecoration(
                          color: _getPainColor(_painScore).withValues(alpha: 0.15),
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: Text(
                          '$_painScore / 10',
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.w800,
                            color: _getPainColor(_painScore),
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 3),
                  Text(
                    _getPainDescriptor(_painScore),
                    style: TextStyle(
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                      color: _getPainColor(_painScore),
                    ),
                  ),
                  const SizedBox(height: 6),
                  SliderTheme(
                    data: SliderTheme.of(context).copyWith(
                      trackHeight: 4,
                      thumbShape: const RoundSliderThumbShape(enabledThumbRadius: 8),
                      overlayShape: const RoundSliderOverlayShape(overlayRadius: 16),
                    ),
                    child: Slider(
                      value: _painScore.toDouble(),
                      min: 0,
                      max: 10,
                      divisions: 10,
                      activeColor: _getPainColor(_painScore),
                      inactiveColor: Colors.grey.shade300,
                      onChanged: (val) => setState(() => _painScore = val.round()),
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 12),

            // Observed Energy Slider
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.grey.shade50,
                borderRadius: BorderRadius.circular(14),
                border: Border.all(color: Colors.grey.shade200),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Energy Level',
                        style: TextStyle(fontSize: 13, fontWeight: FontWeight.bold, color: Color(0xFF0C3C54)),
                      ),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                        decoration: BoxDecoration(
                          color: _getEnergyColor(_energyScore).withValues(alpha: 0.15),
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: Text(
                          '$_energyScore / 10',
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.w800,
                            color: _getEnergyColor(_energyScore),
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 3),
                  Text(
                    _getEnergyDescriptor(_energyScore),
                    style: TextStyle(
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                      color: _getEnergyColor(_energyScore),
                    ),
                  ),
                  const SizedBox(height: 6),
                  SliderTheme(
                    data: SliderTheme.of(context).copyWith(
                      trackHeight: 4,
                      thumbShape: const RoundSliderThumbShape(enabledThumbRadius: 8),
                      overlayShape: const RoundSliderOverlayShape(overlayRadius: 16),
                    ),
                    child: Slider(
                      value: _energyScore.toDouble(),
                      min: 1,
                      max: 10,
                      divisions: 9,
                      activeColor: _getEnergyColor(_energyScore),
                      inactiveColor: Colors.grey.shade300,
                      onChanged: (val) => setState(() => _energyScore = val.round()),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: _submitting ? null : () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),
        ElevatedButton(
          style: ElevatedButton.styleFrom(
            backgroundColor: const Color(0xFF6B7A4D),
            foregroundColor: Colors.white,
            elevation: 0,
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
          ),
          onPressed: _submitting ? null : _submit,
          child: _submitting
              ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
              : Text(widget.existingNote != null ? 'Save Changes' : 'Submit to Physio'),
        ),
      ],
    );
  }
}
