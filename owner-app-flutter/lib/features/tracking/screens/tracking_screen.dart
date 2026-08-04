import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../providers/tracking_provider.dart';

class TrackingScreen extends ConsumerStatefulWidget {
  const TrackingScreen({super.key, required this.petId, required this.petName});

  final int petId;
  final String petName;

  @override
  ConsumerState<TrackingScreen> createState() => _TrackingScreenState();
}

class _TrackingScreenState extends ConsumerState<TrackingScreen> {
  double _pain = 5;
  double _energy = 5;
  double _mobility = 5;
  double _appetite = 5;
  double _lameness = 5;

  @override
  Widget build(BuildContext context) {
    final trackingState = ref.watch(trackingProvider(widget.petId));

    return AppPageScaffold(
      title: 'Daily Tracking',
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 24, 20, 32),
        children: [
          AppPanel(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'How is ${widget.petName} doing today?',
                  style: Theme.of(context).textTheme.titleLarge?.copyWith(
                        color: AppColors.primaryDark,
                        fontWeight: FontWeight.w800,
                      ),
                ),
                const SizedBox(height: 8),
                Text(
                  'Slide each indicator from 1 (low concern) to 10 (high concern).',
                  style: TextStyle(color: AppColors.neutralDark.withValues(alpha: 0.7)),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          AppPanel(
            child: Column(
              children: [
                _buildSlider('Pain', _pain, AppColors.alertRed, (v) => setState(() => _pain = v)),
                _buildSlider(
                  'Energy',
                  _energy,
                  AppColors.successGreen,
                  (v) => setState(() => _energy = v),
                ),
                _buildSlider(
                  'Mobility',
                  _mobility,
                  AppColors.primaryLight,
                  (v) => setState(() => _mobility = v),
                ),
                _buildSlider(
                  'Appetite',
                  _appetite,
                  AppColors.accentAmber,
                  (v) => setState(() => _appetite = v),
                ),
                _buildSlider(
                  'Lameness',
                  _lameness,
                  AppColors.primaryDark,
                  (v) => setState(() => _lameness = v),
                  isLast: true,
                ),
              ],
            ),
          ),
          if (trackingState.error != null) ...[
            const SizedBox(height: 16),
            Text(trackingState.error!, style: const TextStyle(color: AppColors.alertRed)),
          ],
          const SizedBox(height: 24),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: trackingState.isSubmitting
                  ? null
                  : () => ref.read(trackingProvider(widget.petId).notifier).submit(
                        pain: _pain.round(),
                        energy: _energy.round(),
                        mobility: _mobility.round(),
                        appetite: _appetite.round(),
                        lameness: _lameness.round(),
                      ),
              child: trackingState.isSubmitting
                  ? const SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                    )
                  : const Text('SAVE TODAY\'S LOG'),
            ),
          ),
          if (trackingState.lastSaved != null) ...[
            const SizedBox(height: 12),
            Center(
              child: Text(
                'Saved at ${TimeOfDay.fromDateTime(trackingState.lastSaved!).format(context)}',
                style: TextStyle(color: AppColors.successGreen.withValues(alpha: 0.9)),
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
              Text(label, style: const TextStyle(fontWeight: FontWeight.w700)),
              Text(
                '${value.round()}/10',
                style: TextStyle(color: color, fontWeight: FontWeight.w800),
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
