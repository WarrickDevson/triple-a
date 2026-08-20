import 'blob_download_stub.dart'
    if (dart.library.html) 'blob_download_web.dart'
    if (dart.library.io) 'blob_download_mobile.dart' as download_impl;

class FileDownloadUtil {
  static void downloadBytes(List<int> bytes, String fileName, {String mimeType = 'application/pdf'}) {
    download_impl.downloadBlobFile(bytes, fileName, mimeType: mimeType);
  }
}
