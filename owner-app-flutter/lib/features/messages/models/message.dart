class PetMessage {
  const PetMessage({
    required this.messageId,
    required this.messageThreadId,
    required this.senderUserId,
    required this.senderName,
    required this.body,
    this.videoSubmissionId,
    this.readAt,
    required this.createdDate,
  });

  final int messageId;
  final int messageThreadId;
  final int senderUserId;
  final String senderName;
  final String body;
  final int? videoSubmissionId;
  final DateTime? readAt;
  final DateTime createdDate;

  factory PetMessage.fromJson(Map<String, dynamic> json) {
    return PetMessage(
      messageId: json['messageId'] as int,
      messageThreadId: json['messageThreadId'] as int,
      senderUserId: json['senderUserId'] as int,
      senderName: json['senderName'] as String,
      body: json['body'] as String,
      videoSubmissionId: json['videoSubmissionId'] as int?,
      readAt: json['readAt'] != null ? DateTime.parse(json['readAt'] as String) : null,
      createdDate: DateTime.parse(json['createdDate'] as String),
    );
  }
}
