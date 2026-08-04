class ChatMessage {
  const ChatMessage({
    required this.text,
    required this.isUser,
    this.sources = const [],
  });

  final String text;
  final bool isUser;
  final List<ChatSource> sources;
}

class ChatSource {
  const ChatSource({required this.title, required this.excerpt});

  final String title;
  final String excerpt;

  factory ChatSource.fromJson(Map<String, dynamic> json) {
    return ChatSource(
      title: json['title'] as String,
      excerpt: json['excerpt'] as String,
    );
  }
}
