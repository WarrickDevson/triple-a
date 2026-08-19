import 'package:dio/dio.dart';
import 'package:file_picker/file_picker.dart';
import 'package:flutter/foundation.dart';
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

  @override
  void initState() {
    super.initState();
    Future.microtask(() async {
      await ref.read(petsProvider.notifier).loadPets(force: true);
      if (!mounted) return;
      final pets = ref.read(petsProvider).pets;
      if (pets.isNotEmpty && _selectedPet == null) {
        _loadExercisesForPet(pets.first);
      }
    });
  }

  Future<void> _pickVideo() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.video,
        allowMultiple: false,
        withData: kIsWeb,
        withReadStream: kIsWeb,
      );
      if (result == null || result.files.isEmpty) return;

      final file = result.files.first;

      // Validate video format extensions
      final ext = file.extension?.toLowerCase() ?? '';
      final validExtensions = ['mp4', 'mov', 'hevc', 'm4v', 'avi', 'mkv', 'webm', '3gp'];
      if (ext.isNotEmpty && !validExtensions.contains(ext)) {
        setState(() {
          _error = 'Unsupported file format (.$ext). Please select a video file (.mp4, .mov, .hevc, etc.).';
        });
        return;
      }

      setState(() {
        _selectedFile = file;
        _error = null;
      });
    } catch (e) {
      setState(() {
        _error = 'Failed to select video: ${e.toString()}';
      });
    }
  }

  Future<void> _loadExercisesForPet(Pet pet) async {
    setState(() {
      _selectedPet = pet;
      _selectedExercise = null;
    });
    await ref.read(rehabProgramsProvider(pet.petId).notifier).loadPrograms(pet.petId, force: true);
    if (!mounted) return;
    final programsState = ref.read(rehabProgramsProvider(pet.petId));
    final exercises = programsState.activeProgram?.exercises ?? [];
    if (exercises.isNotEmpty) {
      setState(() {
        _selectedExercise = exercises.first;
      });
    }
  }

  String _formatFileSize(int bytes) {
    if (bytes <= 0) return '';
    if (bytes < 1024 * 1024) {
      return '${(bytes / 1024).toStringAsFixed(1)} KB';
    }
    return '${(bytes / (1024 * 1024)).toStringAsFixed(1)} MB';
  }

  Future<void> _upload() async {
    final pet = _selectedPet;
    final exercise = _selectedExercise;
    final file = _selectedFile;

    if (pet == null || exercise == null || file == null) {
      setState(() => _error = 'Select a pet, exercise, and video file.');
      return;
    }

    if (!kIsWeb && file.path == null && file.bytes == null && file.readStream == null) {
      setState(() => _error = 'Unable to read the selected video file.');
      return;
    }

    setState(() {
      _isUploading = true;
      _uploadProgress = 0;
      _error = null;
    });

    try {
      final dio = ref.read(authProvider.notifier).client;
      MultipartFile multipartFile;

      if (kIsWeb) {
        if (file.bytes != null) {
          multipartFile = MultipartFile.fromBytes(file.bytes!, filename: file.name);
        } else if (file.readStream != null) {
          multipartFile = MultipartFile.fromStream(
            () => file.readStream!,
            file.size,
            filename: file.name,
          );
        } else {
          throw Exception('Unable to read selected video data on Web.');
        }
      } else {
        if (file.path != null) {
          multipartFile = await MultipartFile.fromFile(file.path!, filename: file.name);
        } else if (file.bytes != null) {
          multipartFile = MultipartFile.fromBytes(file.bytes!, filename: file.name);
        } else {
          throw Exception('Unable to read selected video file path.');
        }
      }

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
        _selectedFile = null;
        _uploadProgress = 0;
      });
      if (!mounted) return;
      ScaffoldMessenger.of(context).clearSnackBars();
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => const VideoInboxScreen()),
      );
    } on DioException catch (e) {
      final message = e.response?.data is Map
          ? (e.response!.data as Map)['message']?.toString()
          : null;
      setState(() => _error = message ?? 'Upload failed. Please try again.');
    } catch (e) {
      setState(() => _error = 'Upload failed: ${e.toString()}');
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

    if (_selectedPet == null && petsState.pets.isNotEmpty) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (mounted && _selectedPet == null) {
          _loadExercisesForPet(petsState.pets.first);
        }
      });
    } else if (_selectedPet != null && _selectedExercise == null && exercises.isNotEmpty) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (mounted && _selectedExercise == null) {
          setState(() {
            _selectedExercise = exercises.first;
          });
        }
      });
    }

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
                  initialValue: petsState.pets.any((p) => p.petId == _selectedPet?.petId)
                      ? petsState.pets.firstWhere((p) => p.petId == _selectedPet!.petId)
                      : null,
                  decoration: const InputDecoration(labelText: 'Pet'),
                  items: petsState.pets
                      .map((pet) => DropdownMenuItem(value: pet, child: Text(pet.petName)))
                      .toList(),
                  onChanged:
                      _isUploading ? null : (pet) => pet != null ? _loadExercisesForPet(pet) : null,
                ),
                const SizedBox(height: 16),
                DropdownButtonFormField<RehabProgramExercise>(
                  initialValue: exercises.any((ex) => ex.exerciseId == _selectedExercise?.exerciseId)
                      ? exercises.firstWhere((ex) => ex.exerciseId == _selectedExercise!.exerciseId)
                      : null,
                  decoration: const InputDecoration(labelText: 'Exercise'),
                  items: exercises
                      .map((ex) => DropdownMenuItem(value: ex, child: Text(ex.title)))
                      .toList(),
                  onChanged: _isUploading
                      ? null
                      : (ex) => setState(() => _selectedExercise = ex),
                ),
                const SizedBox(height: 20),
                if (_selectedFile != null) ...[
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                    decoration: BoxDecoration(
                      color: AppColors.primaryLight.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(8),
                      border: Border.all(color: AppColors.primaryLight.withValues(alpha: 0.3)),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.check_circle_rounded, color: AppColors.primaryLight),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                _selectedFile!.name,
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                                style: const TextStyle(
                                  fontWeight: FontWeight.bold,
                                  color: AppColors.neutralDark,
                                ),
                              ),
                              if (_selectedFile!.size > 0)
                                Text(
                                  _formatFileSize(_selectedFile!.size),
                                  style: TextStyle(
                                    fontSize: 12,
                                    color: AppColors.neutralDark.withValues(alpha: 0.6),
                                  ),
                                ),
                            ],
                          ),
                        ),
                        if (!_isUploading)
                          IconButton(
                            icon: const Icon(Icons.close_rounded, size: 20),
                            onPressed: () => setState(() => _selectedFile = null),
                            tooltip: 'Change video',
                          ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 12),
                ],
                SizedBox(
                  width: double.infinity,
                  child: OutlinedButton.icon(
                    onPressed: _isUploading ? null : _pickVideo,
                    icon: Icon(_selectedFile == null ? Icons.video_library_outlined : Icons.edit),
                    label: Text(_selectedFile == null ? 'CHOOSE VIDEO (.mp4, .mov, .hevc)' : 'CHANGE VIDEO'),
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

