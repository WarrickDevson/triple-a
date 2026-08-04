import 'package:dio/dio.dart';
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/app_chrome.dart';
import '../../auth/providers/auth_provider.dart';
import '../../exercises/models/rehab_program.dart';
import '../../exercises/providers/exercise_providers.dart';
import '../../pets/models/pet.dart';
import '../../pets/providers/pets_provider.dart';
import 'video_inbox_screen.dart';

class VideoUploadScreen extends ConsumerStatefulWidget {
  const VideoUploadScreen({super.key});

  @override
  ConsumerState<VideoUploadScreen> createState() => _VideoUploadScreenState();
}

class _VideoUploadScreenState extends ConsumerState<VideoUploadScreen> {
  Pet? _selectedPet;
  RehabProgramExercise? _selectedExercise;
  PlatformFile? _selectedFile;
  double _uploadProgress = 0;
  bool _isUploading = false;
  String? _error;
  String? _successMessage;

  @override
  void initState() {
    super.initState();
    Future.microtask(() => ref.read(petsProvider.notifier).loadPets());
  }

  Future<void> _pickVideo() async {
    final result = await FilePicker.platform.pickFiles(
      type: FileType.video,
      allowedExtensions: const ['mp4', 'mov', 'hevc'],
      withData: true,
      withReadStream: true,
    );
    if (result == null || result.files.isEmpty) return;
    setState(() {
      _selectedFile = result.files.first;
      _error = null;
      _successMessage = null;
    });
  }

  Future<void> _loadExercisesForPet(Pet pet) async {
    setState(() {
      _selectedPet = pet;
      _selectedExercise = null;
    });
    await ref.read(rehabProgramsProvider(pet.petId).notifier).loadPrograms(pet.petId, force: true);
  }

  Future<void> _upload() async {
    final pet = _selectedPet;
    final exercise = _selectedExercise;
    final file = _selectedFile;

    if (pet == null || exercise == null || file == null) {
      setState(() => _error = 'Select a pet, exercise, and video file.');
      return;
    }

    if (file.path == null && file.bytes == null) {
      setState(() => _error = 'Unable to read the selected video file.');
      return;
    }

    setState(() {
      _isUploading = true;
      _uploadProgress = 0;
      _error = null;
      _successMessage = null;
    });

    try {
      final dio = ref.read(authProvider.notifier).client;
      final multipartFile = file.path != null
          ? await MultipartFile.fromFile(file.path!, filename: file.name)
          : MultipartFile.fromBytes(file.bytes!, filename: file.name);

      final formData = FormData.fromMap({
        'exerciseId': exercise.exerciseId,
        'file': multipartFile,
      });

      await dio.post<void>(
        '/api/pets/${pet.petId}/videos',
        data: formData,
        onSendProgress: (sent, total) {
          if (total <= 0) return;
          setState(() => _uploadProgress = sent / total);
        },
      );

      setState(() {
        _successMessage = 'Video uploaded. Your physiotherapist will review it soon.';
        _selectedFile = null;
        _uploadProgress = 0;
      });
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: const Text('Video uploaded successfully.'),
          action: SnackBarAction(
            label: 'View feedback',
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => const VideoInboxScreen()),
            ),
          ),
        ),
      );
    } on DioException catch (e) {
      final message = e.response?.data is Map
          ? (e.response!.data as Map)['message']?.toString()
          : null;
      setState(() => _error = message ?? 'Upload failed. Please try again.');
    } finally {
      setState(() => _isUploading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final petsState = ref.watch(petsProvider);
    final programsState = _selectedPet != null
        ? ref.watch(rehabProgramsProvider(_selectedPet!.petId))
        : const RehabProgramsState();

    final exercises = programsState.activeProgram?.exercises ?? [];

    return AppPageScaffold(
      title: 'Upload Video',
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 24, 20, 32),
        children: [
          AppPanel(
            child: Text(
              'Share a video of your pet performing an assigned exercise for physiotherapist review.',
              style: TextStyle(
                color: AppColors.neutralDark.withValues(alpha: 0.75),
                height: 1.5,
              ),
            ),
          ),
          const SizedBox(height: 16),
          AppPanel(
            child: Column(
              children: [
                DropdownButtonFormField<Pet>(
                  initialValue: _selectedPet,
                  decoration: const InputDecoration(labelText: 'Pet'),
                  items: petsState.pets
                      .map((pet) => DropdownMenuItem(value: pet, child: Text(pet.petName)))
                      .toList(),
                  onChanged:
                      _isUploading ? null : (pet) => pet != null ? _loadExercisesForPet(pet) : null,
                ),
                const SizedBox(height: 16),
                DropdownButtonFormField<RehabProgramExercise>(
                  initialValue: _selectedExercise,
                  decoration: const InputDecoration(labelText: 'Exercise'),
                  items: exercises
                      .map((ex) => DropdownMenuItem(value: ex, child: Text(ex.title)))
                      .toList(),
                  onChanged: _isUploading
                      ? null
                      : (ex) => setState(() => _selectedExercise = ex),
                ),
                const SizedBox(height: 20),
                SizedBox(
                  width: double.infinity,
                  child: OutlinedButton.icon(
                    onPressed: _isUploading ? null : _pickVideo,
                    icon: const Icon(Icons.video_library_outlined),
                    label: Text(_selectedFile?.name ?? 'CHOOSE VIDEO (.mp4, .mov, .hevc)'),
                  ),
                ),
                if (_isUploading || _uploadProgress > 0) ...[
                  const SizedBox(height: 16),
                  LinearProgressIndicator(
                    value: _uploadProgress > 0 ? _uploadProgress : null,
                    color: AppColors.primaryLight,
                    backgroundColor: AppColors.neutralGrey,
                  ),
                  const SizedBox(height: 8),
                  Text('${(_uploadProgress * 100).round()}% uploaded'),
                ],
                if (_error != null) ...[
                  const SizedBox(height: 16),
                  Text(_error!, style: const TextStyle(color: AppColors.alertRed)),
                ],
                if (_successMessage != null) ...[
                  const SizedBox(height: 16),
                  Text(_successMessage!, style: const TextStyle(color: AppColors.successGreen)),
                ],
              ],
            ),
          ),
          const SizedBox(height: 24),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: _isUploading ? null : _upload,
              child: Text(_isUploading ? 'UPLOADING...' : 'UPLOAD VIDEO'),
            ),
          ),
        ],
      ),
    );
  }
}
