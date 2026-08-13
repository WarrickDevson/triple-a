import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../models/pet.dart';
import '../providers/owner_notes_provider.dart';

class SubmitOwnerNoteDialog extends ConsumerStatefulWidget {
  const SubmitOwnerNoteDialog({super.key, required this.pet});

  final Pet pet;

  @override
  ConsumerState<SubmitOwnerNoteDialog> createState() => _SubmitOwnerNoteDialogState();
}

class _SubmitOwnerNoteDialogState extends ConsumerState<SubmitOwnerNoteDialog> {
  final _notesController = TextEditingController();
  int _painScore = 3;
  int _energyScore = 4;
  bool _submitting = false;

  @override
  void dispose() {
    _notesController.dispose();
    super.dispose();
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

    final ok = await ref.read(ownerNotesProvider.notifier).submitOwnerSubjectiveNote(
          petId: widget.pet.petId,
          notes: text,
          painObserved: _painScore,
          energyObserved: _energyScore,
        );

    setState(() => _submitting = false);

    if (ok && mounted) {
      ref.invalidate(ownerNotesListProvider(widget.pet.petId));
      Navigator.of(context).pop();
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Your notes were submitted for your physio\'s review!')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      title: Row(
        children: [
          const Icon(Icons.rate_review_outlined, color: Color(0xFF6B7A4D)),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              'Share Notes with Physio',
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
              'Describe what you observed at home for ${widget.pet.petName} (e.g. stiffness after walks, appetite, new behaviors). Your physio will review this during your SOAP session.',
              style: TextStyle(fontSize: 13, color: Colors.grey.shade700, height: 1.4),
            ),
            const SizedBox(height: 16),
            TextField(
              controller: _notesController,
              maxLines: 4,
              decoration: InputDecoration(
                hintText: 'e.g. Buddy seems a little stiff on the right hind leg after long walks...',
                filled: true,
                fillColor: Colors.grey.shade100,
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: BorderSide.none,
                ),
              ),
            ),
            const SizedBox(height: 16),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text('Observed Pain (1-10):', style: TextStyle(fontSize: 13, fontWeight: FontWeight.w600)),
                Text('$_painScore/10', style: const TextStyle(fontSize: 14, fontWeight: FontWeight.bold, color: Color(0xFF6B7A4D))),
              ],
            ),
            Slider(
              value: _painScore.toDouble(),
              min: 0,
              max: 10,
              divisions: 10,
              activeColor: const Color(0xFF6B7A4D),
              onChanged: (val) => setState(() => _painScore = val.round()),
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
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
          ),
          onPressed: _submitting ? null : _submit,
          child: _submitting
              ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
              : const Text('Submit to Physio'),
        ),
      ],
    );
  }
}
