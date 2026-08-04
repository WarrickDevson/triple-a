import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/utils/formatters.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../../core/widgets/pet_avatar.dart';
import '../../../core/widgets/section_card.dart';
import '../providers/pets_provider.dart';
import 'add_pet_screen.dart';
import 'pet_detail_screen.dart';

class PetsListScreen extends ConsumerWidget {
  const PetsListScreen({super.key, this.embedded = false});

  final bool embedded;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final petsState = ref.watch(petsProvider);

    final body = RefreshIndicator(
      color: AppColors.sage,
      onRefresh: () => ref.read(petsProvider.notifier).loadPets(force: true),
      child: _buildBody(context, petsState),
    );

    if (embedded) {
      return PageWashBackground(
        child: SafeArea(
          child: Column(
            children: [
              const ShellHeader(title: 'My Pets', showLogo: false),
              Expanded(child: body),
            ],
          ),
        ),
      );
    }

    return AppPageScaffold(
      title: 'My Pets',
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => Navigator.of(context).push(
          MaterialPageRoute(builder: (_) => const AddPetScreen()),
        ),
        label: const Text('Add Pet'),
        icon: const Icon(Icons.add),
      ),
      body: body,
    );
  }

  Widget _buildBody(BuildContext context, PetsState petsState) {
    if (petsState.isLoading && petsState.pets.isEmpty) {
      return const Center(child: CircularProgressIndicator());
    }

    if (petsState.error != null && petsState.pets.isEmpty) {
      return ListView(
        padding: const EdgeInsets.all(24),
        children: [
          AppEmptyState(
            icon: Icons.error_outline,
            title: 'Unable to load pets',
            message: petsState.error,
          ),
        ],
      );
    }

    if (petsState.pets.isEmpty) {
      return ListView(
        padding: const EdgeInsets.all(24),
        children: [
          const SizedBox(height: 48),
          AppEmptyState(
            icon: Icons.pets_rounded,
            title: 'No pets yet',
            message: 'Add your first companion to start rehabilitation tracking.',
            action: ElevatedButton(
              onPressed: () => Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const AddPetScreen()),
              ),
              child: const Text('Add Pet Profile'),
            ),
          ),
        ],
      );
    }

    return ListView.separated(
      padding: const EdgeInsets.fromLTRB(20, 8, 20, 100),
      itemCount: petsState.pets.length + 1,
      separatorBuilder: (_, __) => const SizedBox(height: 10),
      itemBuilder: (context, index) {
        if (index == petsState.pets.length) {
          return SectionCard(
            onTap: () => Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => const AddPetScreen()),
            ),
            child: const Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.add_circle_outline, color: AppColors.sage),
                SizedBox(width: 8),
                Text(
                  'Add New Pet',
                  style: TextStyle(fontWeight: FontWeight.w700, color: AppColors.sage),
                ),
              ],
            ),
          );
        }

        final pet = petsState.pets[index];
        final progress = placeholderWeeklyProgress(pet.petId);
        final subtitle = formatPetSubtitle(breed: pet.breed, birthDate: pet.birthDate);

        return SectionCard(
          onTap: () => Navigator.of(context).push(
            MaterialPageRoute(builder: (_) => PetDetailScreen(pet: pet)),
          ),
          child: Row(
            children: [
              PetAvatar(name: pet.petName, species: pet.species, size: 52),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      pet.petName,
                      style: const TextStyle(
                        fontWeight: FontWeight.w800,
                        color: AppColors.navy,
                        fontSize: 16,
                      ),
                    ),
                    if (subtitle.isNotEmpty)
                      Text(
                        subtitle,
                        style: const TextStyle(color: AppColors.neutralMuted, fontSize: 13),
                      ),
                    const SizedBox(height: 8),
                    ClipRRect(
                      borderRadius: BorderRadius.circular(4),
                      child: LinearProgressIndicator(
                        value: progress / 100,
                        minHeight: 5,
                        backgroundColor: AppColors.neutralGrey,
                        color: AppColors.sage,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 12),
              Text(
                '${progress.round()}%',
                style: const TextStyle(
                  fontWeight: FontWeight.w800,
                  color: AppColors.sage,
                ),
              ),
              Icon(Icons.chevron_right_rounded, color: AppColors.navy.withValues(alpha: 0.3)),
            ],
          ),
        );
      },
    );
  }
}
