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

  String _uploadType = 'exercise';
  final _titleController = TextEditingController();
  final _notesController = TextEditingController();

  @override
  void initState() {
    super.initState();
    Future.microtask(() async {
      await ref.read(petsProvider.notifier).loadPets();
      if (!mounted) return;
      final pets = ref.read(petsProvider).pets;
      if (pets.isNotEmpty && _selectedPet == null) {
        _loadExercisesForPet(pets.first);
      }
    });
  }

  @override
  void dispose() {
    _titleController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  Future<void> _pickVideo() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.video,
        allowMultiple: false,
        withData: kIsWeb,
        withReadStream: false,
      );
      if (result == null || result.files.isEmpty) return;

      final file = result.files.first;

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
    await ref.read(rehabProgramsProvider(pet.petId).notifier).loadPrograms(pet.petId);
    if (!mounted) return;
    final programsState = ref.read(rehabProgramsProvider(pet.petId));
    final exercises = programsState.activeProgram?.exercises ?? [];
    if (exercises.isNotEmpty) {
      setState(() {
        _selectedExercise = exercises.first;
      });
    } else {
      setState(() {
        _uploadType = 'progress';
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

    if (pet == null) {
      setState(() => _error = 'Please select a pet.');
      return;
    }

    if (file == null) {
      setState(() => _error = 'Please select a video to upload.');
      return;
    }

    if (_uploadType == 'exercise' && exercise == null) {
      setState(() => _error = 'Please select an exercise or switch to General Progress.');
      return;
    }

    setState(() {
      _isUploading = true;
      _uploadProgress = 0;
      _error = null;
    });

    try {
      final dio = ref.read(authProvider.notifier).client;

      String fileName = file.name;
      if (!fileName.contains('.')) {
        fileName = '$fileName.mp4';
      }

      MultipartFile multipartFile;

      if (kIsWeb) {
        if (file.bytes != null) {
          multipartFile = MultipartFile.fromBytes(file.bytes!, filename: fileName);
        } else if (file.readStream != null) {
          multipartFile = MultipartFile.fromStream(
            () => file.readStream!,
            file.size,
            filename: fileName,
          );
        } else {
          throw Exception('Unable to read selected video data on Web.');
        }
      } else {
        if (file.path != null) {
          multipartFile = await MultipartFile.fromFile(file.path!, filename: fileName);
        } else if (file.bytes != null) {
          multipartFile = MultipartFile.fromBytes(file.bytes!, filename: fileName);
        } else {
          throw Exception('Unable to read selected video file path.');
        }
      }

      final formMap = <String, dynamic>{
        'file': multipartFile,
      };

      if (_uploadType == 'exercise' && exercise != null) {
        formMap['exerciseId'] = exercise.exerciseId;
      }

      final title = _titleController.text.trim();
      if (title.isNotEmpty) {
        formMap['title'] = title;
      } else if (_uploadType == 'progress') {
        formMap['title'] = 'Progress Video (${DateTime.now().month}/${DateTime.now().day})';
      }

      final notes = _notesController.text.trim();
      if (notes.isNotEmpty) {
        formMap['notes'] = notes;
      }

      final formData = FormData.fromMap(formMap);

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
        _titleController.clear();
        _notesController.clear();
      });
      if (!mounted) return;
      ScaffoldMessenger.of(context).clearSnackBars();
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => VideoInboxScreen(initialPetId: pet.petId)),
      );
    } on DioException catch (e) {
      debugPrint('Video upload DioException: ${e.response?.statusCode} ${e.response?.data} ${e.message}');
      final message = e.response?.data is Map
          ? (e.response!.data as Map)['message']?.toString()
          : null;
      setState(() => _error = message ?? (e.response?.statusMessage ?? e.message ?? 'Upload failed. Please try again.'));
    } catch (e) {
      debugPrint('Video upload error: $e');
      setState(() => _error = 'Upload failed: ${e.toString()}');
    } finally {
      setState(() => _isUploading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final petsState = ref.watch(petsProvider);
    final pets = petsState.pets;

    final selectedPet = _selectedPet ?? (pets.isNotEmpty ? pets.first : null);
    final programsState = selectedPet != null ? ref.watch(rehabProgramsProvider(selectedPet.petId)) : null;
    final exercises = programsState?.activeProgram?.exercises ?? [];

    return AppPageScaffold(
      title: 'Upload Video',
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 20, 20, 32),
        children: [
          AppPanel(
            child: Text(
              'Share a video of your pet performing an assigned exercise or daily movement for physiotherapist review.',
              style: TextStyle(
                color: AppColors.neutralDark.withValues(alpha: 0.75),
                height: 1.5,
              ),
            ),
          ),
          const SizedBox(height: 16),

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
            SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: Row(
                children: pets.map((pet) {
                  final isSelected = pet.petId == _selectedPet?.petId;
                  return Padding(
                    padding: const EdgeInsets.only(right: 8),
                    child: Material(
                      color: Colors.transparent,
                      child: InkWell(
                        onTap: _isUploading ? null : () => _loadExercisesForPet(pet),
                        borderRadius: BorderRadius.circular(20),
                        child: Container(
                          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                          decoration: BoxDecoration(
                            color: isSelected ? AppColors.primaryDark : Colors.white,
                            borderRadius: BorderRadius.circular(20),
                            border: Border.all(
                              color: isSelected ? AppColors.primaryDark : AppColors.neutralGrey,
                              width: 1.5,
                            ),
                          ),
                          child: Text(
                            pet.petName,
                            style: TextStyle(
                              color: isSelected ? Colors.white : AppColors.neutralDark,
                              fontWeight: isSelected ? FontWeight.w800 : FontWeight.w600,
                              fontSize: 13,
                            ),
                          ),
                        ),
                      ),
                    ),
                  );
                }).toList(),
              ),
            ),
            const SizedBox(height: 16),
          ] else if (pets.length == 1) ...[
            AppPanel(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              child: Row(
                children: [
                  const Icon(Icons.pets, size: 20, color: AppColors.primaryLight),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Text(
                      'Uploading for ${pets.first.petName}',
                      style: const TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w700,
                        color: AppColors.primaryDark,
                      ),
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),
          ],

          AppPanel(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                SegmentedButton<String>(
                  segments: [
                    ButtonSegment<String>(
                      value: 'exercise',
                      label: const Text('Assigned Exercise'),
                      icon: const Icon(Icons.fitness_center_outlined),
                      enabled: exercises.isNotEmpty,
                    ),
                    const ButtonSegment<String>(
                      value: 'progress',
                      label: Text('General Progress'),
                      icon: Icon(Icons.trending_up_outlined),
                    ),
                  ],
                  selected: {_uploadType},
                  onSelectionChanged: _isUploading
                      ? null
                      : (newSelection) {
                          setState(() {
                            _uploadType = newSelection.first;
                          });
                        },
                ),
                const SizedBox(height: 16),

                if (_uploadType == 'exercise') ...[
                  if (exercises.isNotEmpty) ...[
                    const Text(
                      'Assigned Exercise',
                      style: TextStyle(
                        fontSize: 12,
                        fontWeight: FontWeight.w700,
                        color: AppColors.neutralDark,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                      decoration: BoxDecoration(
                        border: Border.all(color: AppColors.neutralGrey),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: DropdownButtonHideUnderline(
                        child: DropdownButton<int>(
                          value: exercises.any((ex) => ex.exerciseId == _selectedExercise?.exerciseId)
                              ? _selectedExercise?.exerciseId
                              : exercises.first.exerciseId,
                          isExpanded: true,
                          items: exercises
                              .map(
                                (ex) => DropdownMenuItem<int>(
                                  value: ex.exerciseId,
                                  child: Text(
                                    ex.title,
                                    style: const TextStyle(fontWeight: FontWeight.w600),
                                  ),
                                ),
                              )
                              .toList(),
                          onChanged: _isUploading
                              ? null
                              : (id) {
                                  if (id != null) {
                                    setState(() {
                                      _selectedExercise = exercises.firstWhere((ex) => ex.exerciseId == id);
                                    });
                                  }
                                },
                        ),
                      ),
                    ),
                  ] else
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: AppColors.neutralGrey.withValues(alpha: 0.5),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: const Row(
                        children: [
                          Icon(Icons.info_outline, color: AppColors.primaryLight, size: 20),
                          SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              'No assigned exercises found for this pet. Please use General Progress.',
                              style: TextStyle(fontSize: 13),
                            ),
                          ),
                        ],
                      ),
                    ),
                ] else ...[
                  TextField(
                    controller: _titleController,
                    enabled: !_isUploading,
                    decoration: const InputDecoration(
                      labelText: 'Title (optional)',
                      hintText: 'e.g. Walking in garden, Standing on three legs',
                      border: OutlineInputBorder(),
                    ),
                  ),
                ],

                const SizedBox(height: 16),
                TextField(
                  controller: _notesController,
                  enabled: !_isUploading,
                  maxLines: 3,
                  decoration: const InputDecoration(
                    labelText: 'Notes for Physiotherapist (optional)',
                    hintText: 'e.g. He seemed a little hesitant on the left leg today.',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 16),

                InkWell(
                  onTap: _isUploading ? null : _pickVideo,
                  borderRadius: BorderRadius.circular(8),
                  child: Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      border: Border.all(
                        color: _selectedFile != null ? AppColors.successGreen : AppColors.primaryLight,
                        width: 1.5,
                      ),
                      borderRadius: BorderRadius.circular(8),
                      color: _selectedFile != null
                          ? AppColors.successGreen.withValues(alpha: 0.05)
                          : AppColors.primaryLight.withValues(alpha: 0.05),
                    ),
                    child: Column(
                      children: [
                        Icon(
                          _selectedFile != null ? Icons.check_circle_outline : Icons.cloud_upload_outlined,
                          size: 36,
                          color: _selectedFile != null ? AppColors.successGreen : AppColors.primaryLight,
                        ),
                        const SizedBox(height: 8),
                        Text(
                          _selectedFile != null ? _selectedFile!.name : 'Choose Video from Device',
                          style: TextStyle(
                            fontWeight: FontWeight.w700,
                            color: _selectedFile != null ? AppColors.successGreen : AppColors.primaryLight,
                          ),
                          textAlign: TextAlign.center,
                        ),
                        if (_selectedFile != null) ...[
                          const SizedBox(height: 4),
                          Text(
                            _formatFileSize(_selectedFile!.size),
                            style: TextStyle(
                              fontSize: 12,
                              color: AppColors.neutralDark.withValues(alpha: 0.6),
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                ),

                if (_isUploading) ...[
                  const SizedBox(height: 16),
                  LinearProgressIndicator(value: _uploadProgress > 0 ? _uploadProgress : null),
                  const SizedBox(height: 8),
                  Center(
                    child: Text(
                      _uploadProgress > 0
                          ? 'Uploading ${(_uploadProgress * 100).toStringAsFixed(0)}%...'
                          : 'Preparing upload...',
                      style: const TextStyle(fontSize: 12, color: AppColors.neutralDark),
                    ),
                  ),
                ],

                if (_error != null) ...[
                  const SizedBox(height: 12),
                  Text(
                    _error!,
                    style: const TextStyle(color: AppColors.alertRed, fontSize: 13),
                    textAlign: TextAlign.center,
                  ),
                ],

                const SizedBox(height: 20),
                ElevatedButton(
                  onPressed: _isUploading ? null : _upload,
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 14),
                  ),
                  child: _isUploading
                      ? const SizedBox(
                          height: 20,
                          width: 20,
                          child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                        )
                      : const Text('UPLOAD VIDEO', style: TextStyle(fontWeight: FontWeight.w800)),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
