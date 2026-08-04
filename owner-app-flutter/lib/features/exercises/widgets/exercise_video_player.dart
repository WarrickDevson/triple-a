import 'package:chewie/chewie.dart';
import 'package:flutter/material.dart';
import 'package:video_player/video_player.dart';

class ExerciseVideoPlayer extends StatefulWidget {
  const ExerciseVideoPlayer({super.key, required this.videoUrl});

  final String videoUrl;

  @override
  State<ExerciseVideoPlayer> createState() => _ExerciseVideoPlayerState();
}

class _ExerciseVideoPlayerState extends State<ExerciseVideoPlayer> {
  VideoPlayerController? _videoController;
  ChewieController? _chewieController;
  bool _isInitializing = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _initialize();
  }

  Future<void> _initialize() async {
    try {
      final controller = VideoPlayerController.networkUrl(Uri.parse(widget.videoUrl));
      await controller.initialize();
      final chewie = ChewieController(
        videoPlayerController: controller,
        autoPlay: false,
        looping: false,
        allowMuting: true,
        showControls: true,
      );
      if (!mounted) return;
      setState(() {
        _videoController = controller;
        _chewieController = chewie;
        _isInitializing = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _error = 'Unable to load exercise video.';
        _isInitializing = false;
      });
    }
  }

  @override
  void dispose() {
    _chewieController?.dispose();
    _videoController?.dispose();
    super.dispose();
  }

  void _replay() {
    _videoController?.seekTo(Duration.zero);
    _videoController?.play();
  }

  @override
  Widget build(BuildContext context) {
    if (_isInitializing) {
      return const SizedBox(
        height: 200,
        child: Center(child: CircularProgressIndicator()),
      );
    }

    if (_error != null) {
      return SizedBox(
        height: 200,
        child: Center(child: Text(_error!)),
      );
    }

    return Column(
      children: [
        AspectRatio(
          aspectRatio: _videoController!.value.aspectRatio == 0
              ? 16 / 9
              : _videoController!.value.aspectRatio,
          child: Chewie(controller: _chewieController!),
        ),
        const SizedBox(height: 8),
        Align(
          alignment: Alignment.centerRight,
          child: TextButton(onPressed: _replay, child: const Text('REPLAY')),
        ),
      ],
    );
  }
}
