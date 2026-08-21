import 'blob_download_stub.dart'
    if (dart.library.html) 'blob_download_web.dart'
    if (dart.library.io) 'blob_download_mobile.dart' as download_impl;

class FileDownloadUtil {
  static Future<bool> downloadBytes(List<int> bytes, String fileName, {String mimeType = 'application/pdf'}) async {
    return await download_impl.downloadBlobFile(bytes, fileName, mimeType: mimeType);
  }
}
