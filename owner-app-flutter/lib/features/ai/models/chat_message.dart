class ChatMessage {
  const ChatMessage({
    required this.text,
    required this.isUser,
    this.sources = const [],
    this.attachmentUrl,
    this.attachmentName,
    this.attachmentPath,
    this.attachmentType,
  });

  final String text;
  final bool isUser;
  final List<ChatSource> sources;
  final String? attachmentUrl;
  final String? attachmentName;
  final String? attachmentPath;
  final String? attachmentType;

  Map<String, dynamic> toJson() => {
    'text': text,
    'isUser': isUser,
    'sources': sources.map((s) => s.toJson()).toList(),
    'attachmentUrl': attachmentUrl,
    'attachmentName': attachmentName,
    'attachmentPath': attachmentPath,
    'attachmentType': attachmentType,
  };

  factory ChatMessage.fromJson(Map<String, dynamic> json) {
    return ChatMessage(
      text: json['text'] as String? ?? '',
      isUser: json['isUser'] as bool? ?? false,
      sources: (json['sources'] as List<dynamic>? ?? [])
          .map((s) => ChatSource.fromJson(s as Map<String, dynamic>))
          .toList(),
      attachmentUrl: json['attachmentUrl'] as String?,
      attachmentName: json['attachmentName'] as String?,
      attachmentPath: json['attachmentPath'] as String?,
      attachmentType: json['attachmentType'] as String?,
    );
  }
}

class ChatSource {
  const ChatSource({
    required this.title,
    required this.excerpt,
    this.documentUrl,
    this.downloadUrl,
  });

  final String title;
  final String excerpt;
  final String? documentUrl;
  final String? downloadUrl;

  String get content => excerpt;

  Map<String, dynamic> toJson() => {
    'title': title,
    'excerpt': excerpt,
    'documentUrl': documentUrl,
    'downloadUrl': downloadUrl,
  };

  factory ChatSource.fromJson(Map<String, dynamic> json) {
    return ChatSource(
      title: json['title'] as String? ?? '',
      excerpt: json['excerpt'] as String? ?? '',
      documentUrl: json['documentUrl'] as String?,
      downloadUrl: json['downloadUrl'] as String?,
    );
  }
}
