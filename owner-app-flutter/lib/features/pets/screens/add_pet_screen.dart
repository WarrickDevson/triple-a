import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../models/pet.dart';
import '../providers/pets_provider.dart';

class AddPetScreen extends ConsumerStatefulWidget {
  const AddPetScreen({super.key});

  @override
  ConsumerState<AddPetScreen> createState() => _AddPetScreenState();
}

class _AddPetScreenState extends ConsumerState<AddPetScreen> {
  final _formKey = GlobalKey<FormState>();
  final _petNameController = TextEditingController();
  final _breedController = TextEditingController();
  final _weightController = TextEditingController();
  final _diagnosisController = TextEditingController();
  final _conditionController = TextEditingController();

  String _species = petSpecies.first;
  DateTime? _birthDate;
  String? _formError;

  @override
  void dispose() {
    _petNameController.dispose();
    _breedController.dispose();
    _weightController.dispose();
    _diagnosisController.dispose();
    _conditionController.dispose();
    super.dispose();
  }

  Future<void> _pickBirthDate() async {
    final now = DateTime.now();
    final picked = await showDatePicker(
      context: context,
      initialDate: now,
      firstDate: DateTime(now.year - 30),
      lastDate: now,
    );
    if (picked != null) {
      setState(() => _birthDate = picked);
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    final weight = _weightController.text.trim().isEmpty
        ? null
        : double.tryParse(_weightController.text.trim());

    final success = await ref.read(petsProvider.notifier).createPet(
          petName: _petNameController.text.trim(),
          species: _species,
          breed: _breedController.text.trim(),
          birthDate: _birthDate,
          weightKg: weight,
          diagnosis: _diagnosisController.text.trim(),
          injuryOrCondition: _conditionController.text.trim(),
        );

    if (!mounted) return;

    if (success) {
      Navigator.of(context).pop();
      return;
    }

    setState(() {
      _formError = ref.read(petsProvider).error ?? 'Unable to save pet.';
    });
  }

  @override
  Widget build(BuildContext context) {
    final petsState = ref.watch(petsProvider);

    return AppPageScaffold(
      title: 'Add New Pet',
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 24, 20, 32),
          children: [
            AppPanel(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Step 1 · Pet Profile',
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                          color: AppColors.primaryDark,
                          fontWeight: FontWeight.w800,
                        ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Basic identity for your companion.',
                    style: TextStyle(color: AppColors.neutralDark.withValues(alpha: 0.6)),
                  ),
                  const SizedBox(height: 20),
                  TextFormField(
                    controller: _petNameController,
                    decoration: const InputDecoration(labelText: 'Pet Name'),
                    validator: (value) =>
                        value == null || value.trim().isEmpty ? 'Pet name is required' : null,
                  ),
                  const SizedBox(height: 16),
                  DropdownButtonFormField<String>(
                    initialValue: _species,
                    decoration: const InputDecoration(labelText: 'Species'),
                    items: petSpecies
                        .map((species) => DropdownMenuItem(value: species, child: Text(species)))
                        .toList(),
                    onChanged: (value) {
                      if (value != null) setState(() => _species = value);
                    },
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    controller: _breedController,
                    decoration: const InputDecoration(labelText: 'Breed'),
                  ),
                  const SizedBox(height: 16),
                  ListTile(
                    contentPadding: EdgeInsets.zero,
                    title: const Text('Birth Date', style: TextStyle(fontWeight: FontWeight.w600)),
                    subtitle: Text(
                      _birthDate == null
                          ? 'Not set'
                          : '${_birthDate!.year}-${_birthDate!.month.toString().padLeft(2, '0')}-${_birthDate!.day.toString().padLeft(2, '0')}',
                    ),
                    trailing: OutlinedButton(
                      onPressed: _pickBirthDate,
                      style: OutlinedButton.styleFrom(
                        minimumSize: const Size(0, 40),
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                      ),
                      child: const Text('SELECT'),
                    ),
                  ),
                  const SizedBox(height: 8),
                  TextFormField(
                    controller: _weightController,
                    keyboardType: const TextInputType.numberWithOptions(decimal: true),
                    decoration: const InputDecoration(labelText: 'Weight (kg)'),
                    validator: (value) {
                      if (value == null || value.trim().isEmpty) return null;
                      final parsed = double.tryParse(value);
                      if (parsed == null || parsed <= 0) {
                        return 'Weight must be greater than zero';
                      }
                      return null;
                    },
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),
            AppPanel(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Step 2 · Medical History',
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                          color: AppColors.primaryDark,
                          fontWeight: FontWeight.w800,
                        ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Helps tailor rehabilitation guidance.',
                    style: TextStyle(color: AppColors.neutralDark.withValues(alpha: 0.6)),
                  ),
                  const SizedBox(height: 20),
                  TextFormField(
                    controller: _diagnosisController,
                    decoration: const InputDecoration(labelText: 'Diagnosis'),
                    validator: (value) =>
                        value == null || value.trim().isEmpty ? 'Diagnosis is required' : null,
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    controller: _conditionController,
                    decoration: const InputDecoration(labelText: 'Injury or Condition'),
                    maxLines: 3,
                  ),
                ],
              ),
            ),
            if (_formError != null) ...[
              const SizedBox(height: 16),
              Text(_formError!, style: const TextStyle(color: AppColors.alertRed)),
            ],
            const SizedBox(height: 24),
            ElevatedButton(
              onPressed: petsState.isLoading ? null : _submit,
              child: Text(petsState.isLoading ? 'SAVING...' : 'SAVE PET PROFILE'),
            ),
          ],
        ),
      ),
    );
  }
}
