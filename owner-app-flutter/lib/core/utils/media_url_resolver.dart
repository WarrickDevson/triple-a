import '../config/app_config.dart';

/// Resolves any media path or URL into a permanent, non-expiring URL.
/// Handles canonical relative paths (e.g. 'avatars/abc.jpg'), permanent endpoints
/// ('/api/media/view?...'), legacy expired GCS signed URLs, and external URLs.
String? resolveMediaUrl(String? path) {
  if (path == null || path.trim().isEmpty) return null;
  final trimmed = path.trim();

  final base = AppConfig.fromEnvironment().apiBaseUrl.replaceAll(RegExp(r'/+$'), '');

  // Legacy/stale signed URL handling: strip GCS domain and query string
  if (trimmed.contains('storage.googleapis.com')) {
    try {
      final uri = Uri.parse(trimmed);
      var objPath = uri.path.replaceAll(RegExp(r'^/+'), '');
      final slashIdx = objPath.indexOf('/');
      if (slashIdx >= 0) {
        objPath = objPath.substring(slashIdx + 1);
      }
      return '$base/api/media/view?path=${Uri.encodeComponent(objPath)}';
    } catch (_) {
      // Fall through
    }
  }

  // Full external URL (e.g. YouTube, Unsplash) or inline data/blob
  if (trimmed.startsWith('http://') ||
      trimmed.startsWith('https://') ||
      trimmed.startsWith('blob:') ||
      trimmed.startsWith('data:')) {
    return trimmed;
  }

  // Dedicated /api/media route
  if (trimmed.startsWith('/api/media/') || trimmed.startsWith('api/media/')) {
    final cleanPath = trimmed.startsWith('/') ? trimmed : '/$trimmed';
    return '$base$cleanPath';
  }

  // Relative storage path (e.g. avatars/..., pets/..., exercise-videos/..., documents/...)
  final clean = trimmed.replaceAll(RegExp(r'^/+'), '');
  final firstSlash = clean.indexOf('/');
  final root = firstSlash > 0 ? clean.substring(0, firstSlash).toLowerCase() : clean.toLowerCase();
  const storageFolders = {
    'avatars',
    'pets',
    'exercise-images',
    'exercise-videos',
    'videos',
    'documents',
    'attachments',
    'uploads'
  };

  if (storageFolders.contains(root)) {
    return '$base/api/media/view?path=${Uri.encodeComponent(clean)}';
  }

  final cleanPath = trimmed.startsWith('/') ? trimmed : '/$trimmed';
  return '$base$cleanPath';
}
