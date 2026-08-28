import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../pets/providers/pets_provider.dart';
import '../providers/tracking_provider.dart';

class TrackingScreen extends ConsumerStatefulWidget {
  const TrackingScreen({
    super.key,
    this.petId,
    this.petName,
  });

  final int? petId;
  final String? petName;

  @override
  ConsumerState<TrackingScreen> createState() => _TrackingScreenState();
}

class _TrackingScreenState extends ConsumerState<TrackingScreen> {
  int? _activePetId;
  String? _activePetName;
  int? _syncedPetId;

  double _pain = 5;
  double _energy = 5;
  double _mobility = 5;
  double _appetite = 5;
  double _lameness = 5;

  @override
  void initState() {
    super.initState();
    _activePetId = widget.petId;
    _activePetName = widget.petName;
  }

  @override
  Widget build(BuildContext context) {
    final petsState = ref.watch(petsProvider);
    final pets = petsState.pets;

    // Resolve active pet if not set or invalid
    if (_activePetId == null && pets.isNotEmpty) {
      _activePetId = pets.first.petId;
      _activePetName = pets.first.petName;
    } else if (_activePetId != null && pets.isNotEmpty && !pets.any((p) => p.petId == _activePetId)) {
      _activePetId = pets.first.petId;
      _activePetName = pets.first.petName;
      _syncedPetId = null;
    }

    // If no pets exist
    if (pets.isEmpty && !petsState.isLoading) {
      return AppPageScaffold(
        title: 'Daily Tracking',
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.pets, size: 48, color: AppColors.neutralGrey),
                const SizedBox(height: 12),
                const Text(
                  'No Pets Found',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 8),
                const Text(
                  'Please add a companion first to begin daily activity and mobility tracking.',
                  textAlign: TextAlign.center,
                  style: TextStyle(color: AppColors.neutralDark),
                ),
                const SizedBox(height: 16),
                ElevatedButton(
                  onPressed: () => Navigator.of(context).pop(),
                  child: const Text('Go Back'),
                ),
              ],
            ),
          ),
        ),
      );
    }

    final trackingState = _activePetId != null
        ? ref.watch(trackingProvider(_activePetId!))
        : const TrackingState();

    // Synchronize slider values when active pet changes or today log is loaded
    if (_activePetId != null && _syncedPetId != _activePetId) {
      _pain = trackingState.pain.toDouble();
      _energy = trackingState.energy.toDouble();
      _mobility = trackingState.mobility.toDouble();
      _appetite = trackingState.appetite.toDouble();
      _lameness = trackingState.lameness.toDouble();
      _syncedPetId = _activePetId;
    }

    final currentPetDisplayName = _activePetName ??
        (pets.isNotEmpty ? pets.firstWhere((p) => p.petId == _activePetId, orElse: () => pets.first).petName : 'Pet');

    return AppPageScaffold(
      title: 'Daily Tracking',
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 16, 20, 32),
        children: [
          // -------------------------------------------------------------
          // OPTION A: HORIZONTAL PET SELECTOR CHIPS (IF MULTIPLE PETS)
          // -------------------------------------------------------------
          if (pets.length > 1) ...[
            Padding(
              padding: const EdgeInsets.only(left: 4, bottom: 8),
              child: Text(
                'SELECT COMPANION',
                style: TextStyle(
                  fontSize: 11,
                  fontWeight: FontWeight.w800,
                  letterSpacing: 0.8,
                  color: AppColors.neutralDark.withValues(alpha: 0.6),
                ),
              ),
            ),
            SizedBox(
              height: 44,
              child: ListView.separated(
                scrollDirection: Axis.horizontal,
                itemCount: pets.length,
                separatorBuilder: (_, _) => const SizedBox(width: 8),
                itemBuilder: (context, index) {
                  final pet = pets[index];
                  final isSelected = pet.petId == _activePetId;
                  return Material(
                    color: Colors.transparent,
                    child: InkWell(
                      onTap: () {
                        if (_activePetId != pet.petId) {
                          setState(() {
                            _activePetId = pet.petId;
                            _activePetName = pet.petName;
                            _syncedPetId = null; // force sync from new pet's trackingState
                          });
                        }
                      },
                      borderRadius: BorderRadius.circular(24),
                      child: AnimatedContainer(
                        duration: const Duration(milliseconds: 200),
                        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                        decoration: BoxDecoration(
                          color: isSelected ? AppColors.primaryDark : Colors.white,
                          borderRadius: BorderRadius.circular(24),
                          border: Border.all(
                            color: isSelected ? AppColors.primaryDark : AppColors.neutralGrey,
                            width: 1.5,
                          ),
                          boxShadow: isSelected
                              ? [
                                  BoxShadow(
                                    color: AppColors.primaryDark.withValues(alpha: 0.25),
                                    blurRadius: 6,
                                    offset: const Offset(0, 2),
                                  )
                                ]
                              : null,
                        ),
                        child: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(
                              Icons.pets,
                              size: 15,
                              color: isSelected ? Colors.white : AppColors.primaryDark,
                            ),
                            const SizedBox(width: 7),
                            Text(
                              pet.petName,
                              style: TextStyle(
                                fontSize: 13,
                                fontWeight: FontWeight.w700,
                                color: isSelected ? Colors.white : AppColors.neutralDark,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  );
                },
              ),
            ),
            const SizedBox(height: 16),
          ],

          // -------------------------------------------------------------
          // HEADER PANEL
          // -------------------------------------------------------------
          AppPanel(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: AppColors.primaryLight.withValues(alpha: 0.15),
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(Icons.favorite, size: 18, color: AppColors.primaryDark),
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      child: Text(
                        'How is $currentPetDisplayName doing today?',
                        style: Theme.of(context).textTheme.titleMedium?.copyWith(
                              color: AppColors.primaryDark,
                              fontWeight: FontWeight.w800,
                            ),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                Text(
                  'Slide each indicator from 1 (low concern / normal) to 10 (high concern / severe).',
                  style: TextStyle(
                    fontSize: 12.5,
                    color: AppColors.neutralDark.withValues(alpha: 0.7),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),

          // -------------------------------------------------------------
          // SLIDERS PANEL
          // -------------------------------------------------------------
          AppPanel(
            child: Column(
              children: [
                _buildSlider(
                  'Pain Level',
                  _pain,
                  AppColors.alertRed,
                  (v) => setState(() => _pain = v),
                ),
                _buildSlider(
                  'Energy & Alertness',
                  _energy,
                  AppColors.successGreen,
                  (v) => setState(() => _energy = v),
                ),
                _buildSlider(
                  'Mobility & Movement',
                  _mobility,
                  AppColors.primaryLight,
                  (v) => setState(() => _mobility = v),
                ),
                _buildSlider(
                  'Appetite & Feeding',
                  _appetite,
                  AppColors.accentAmber,
                  (v) => setState(() => _appetite = v),
                ),
                _buildSlider(
                  'Lameness & Stiffness',
                  _lameness,
                  AppColors.primaryDark,
                  (v) => setState(() => _lameness = v),
                  isLast: true,
                ),
              ],
            ),
          ),

          // -------------------------------------------------------------
          // ERROR MESSAGE ALERT
          // -------------------------------------------------------------
          if (trackingState.error != null) ...[
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: AppColors.alertRed.withValues(alpha: 0.08),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: AppColors.alertRed.withValues(alpha: 0.3)),
              ),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Icon(Icons.error_outline, color: AppColors.alertRed, size: 20),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Text(
                      trackingState.error!,
                      style: const TextStyle(
                        color: AppColors.alertRed,
                        fontSize: 13,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],

          // -------------------------------------------------------------
          // SAVE / UPDATE BUTTON
          // -------------------------------------------------------------
          const SizedBox(height: 24),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: (trackingState.isSubmitting || _activePetId == null)
                  ? null
                  : () async {
                      final success = await ref
                          .read(trackingProvider(_activePetId!).notifier)
                          .submit(
                            pain: _pain.round(),
                            energy: _energy.round(),
                            mobility: _mobility.round(),
                            appetite: _appetite.round(),
                            lameness: _lameness.round(),
                          );
                      if (success && context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(
                            backgroundColor: AppColors.primaryDark,
                            content: Text(
                              'Successfully logged daily stats for $currentPetDisplayName!',
                            ),
                            duration: const Duration(seconds: 2),
                          ),
                        );
                      }
                    },
              child: trackingState.isSubmitting
                  ? const SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                    )
                  : Text(trackingState.hasTodayLog ? 'UPDATE TODAY\'S LOG' : 'SAVE TODAY\'S LOG'),
            ),
          ),

          // -------------------------------------------------------------
          // LAST SAVED STATUS
          // -------------------------------------------------------------
          if (trackingState.lastSaved != null) ...[
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
              decoration: BoxDecoration(
                color: AppColors.successGreen.withValues(alpha: 0.08),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Icon(Icons.check_circle, color: AppColors.successGreen, size: 16),
                  const SizedBox(width: 6),
                  Text(
                    'Saved today at ${TimeOfDay.fromDateTime(trackingState.lastSaved!).format(context)}',
                    style: TextStyle(
                      fontSize: 12.5,
                      fontWeight: FontWeight.w600,
                      color: AppColors.successGreen.withValues(alpha: 0.95),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildSlider(
    String label,
    double value,
    Color color,
    ValueChanged<double> onChanged, {
    bool isLast = false,
  }) {
    return Padding(
      padding: EdgeInsets.only(bottom: isLast ? 0 : 20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(label, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 13.5)),
              Text(
                '${value.round()}/10',
                style: TextStyle(color: color, fontWeight: FontWeight.w800, fontSize: 14),
              ),
            ],
          ),
          Slider(
            value: value,
            min: 1,
            max: 10,
            divisions: 9,
            activeColor: color,
            label: value.round().toString(),
            onChanged: onChanged,
          ),
        ],
      ),
    );
  }
}
