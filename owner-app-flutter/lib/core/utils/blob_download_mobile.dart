import 'dart:convert';
import 'package:url_launcher/url_launcher.dart';

void downloadBlobFile(List<int> bytes, String fileName, {String mimeType = 'application/pdf'}) {
  final base64Data = base64Encode(bytes);
  final uri = Uri.parse('data:$mimeType;base64,$base64Data');
  launchUrl(uri, mode: LaunchMode.externalApplication);
}
