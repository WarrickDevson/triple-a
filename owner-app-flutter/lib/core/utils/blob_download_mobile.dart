import 'dart:typed_data';
import 'package:file_picker/file_picker.dart';

Future<bool> downloadBlobFile(List<int> bytes, String fileName, {String mimeType = 'application/pdf'}) async {
  try {
    final saveResult = await FilePicker.platform.saveFile(
      dialogTitle: 'Save Report ($fileName)',
      fileName: fileName,
      bytes: Uint8List.fromList(bytes),
      type: FileType.custom,
      allowedExtensions: ['pdf', 'doc', 'docx', 'png', 'jpg', 'jpeg'],
    );
    return saveResult != null;
  } catch (_) {
    return false;
  }
}
