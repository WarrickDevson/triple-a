import 'package:chewie/chewie.dart';
import 'package:flutter/material.dart';
import 'package:video_player/video_player.dart';
import 'package:youtube_player_iframe/youtube_player_iframe.dart';

import '../../../core/utils/media_url_resolver.dart';

class ExerciseVideoPlayer extends StatefulWidget {
  const ExerciseVideoPlayer({super.key, required this.videoUrl});

  final String videoUrl;

  @override
  State<ExerciseVideoPlayer> createState() => _ExerciseVideoPlayerState();
}

class _ExerciseVideoPlayerState extends State<ExerciseVideoPlayer> {
  VideoPlayerController? _videoController;
  ChewieController? _chewieController;
  YoutubePlayerController? _ytController;
  bool _isInitializing = true;
  String? _error;
  String? _youTubeId;

  @override
  void initState() {
    super.initState();
    _checkAndInit();
  }

  @override
  void didUpdateWidget(covariant ExerciseVideoPlayer oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.videoUrl != widget.videoUrl) {
      _cleanup();
      _checkAndInit();
    }
  }

  void _cleanup() {
    _ytController?.close();
    _ytController = null;
    _chewieController?.dispose();
    _chewieController = null;
    _videoController?.dispose();
    _videoController = null;
    _error = null;
    _youTubeId = null;
    _isInitializing = true;
  }

  static String? _extractYouTubeId(String url) {
    final regExp = RegExp(
      r'(?:youtu\.be\/|youtube\.com\/(?:embed\/|v\/|watch\?v=|watch\?.+&v=|shorts\/))([\w-]{11})',
      caseSensitive: false,
    );
    final match = regExp.firstMatch(url);
    return match?.group(1);
  }

  String get _effectiveVideoUrl {
    final raw = widget.videoUrl.trim();
    return resolveMediaUrl(raw) ?? raw;
  }

  void _checkAndInit() {
    final ytId = _extractYouTubeId(widget.videoUrl);
    if (ytId != null) {
      _ytController = YoutubePlayerController.fromVideoId(
        videoId: ytId,
        autoPlay: false,
        params: const YoutubePlayerParams(
          showFullscreenButton: true,
          showControls: true,
          mute: false,
        ),
      );
      setState(() {
        _youTubeId = ytId;
        _isInitializing = false;
      });
      return;
    }
    _initializeDirectVideo();
  }

  Future<void> _initializeDirectVideo() async {
    try {
      final url = _effectiveVideoUrl;
      final controller = VideoPlayerController.networkUrl(Uri.parse(url));
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
    _ytController?.close();
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

    if (_youTubeId != null && _ytController != null) {
      return ClipRRect(
        borderRadius: BorderRadius.circular(12),
        child: Container(
          color: Colors.black,
          child: YoutubePlayer(
            controller: _ytController!,
            aspectRatio: 16 / 9,
          ),
        ),
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
