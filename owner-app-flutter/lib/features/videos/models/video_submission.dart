class VideoSubmission {
  const VideoSubmission({
    required this.videoSubmissionId,
    required this.petId,
    required this.petName,
    this.exerciseId,
    this.exerciseTitle,
    this.title,
    this.notes,
    required this.rawVideoStorageUrl,
    this.processedVideoStreamingUrl,
    required this.processingStatus,
    required this.isReviewed,
    this.physioFeedbackNotes,
    required this.createdDate,
  });

  final int videoSubmissionId;
  final int petId;
  final String petName;
  final int? exerciseId;
  final String? exerciseTitle;
  final String? title;
  final String? notes;
  final String rawVideoStorageUrl;
  final String? processedVideoStreamingUrl;
  final String processingStatus;
  final bool isReviewed;
  final String? physioFeedbackNotes;
  final DateTime createdDate;

  String get displayTitle {
    if (exerciseTitle != null && exerciseTitle!.trim().isNotEmpty) {
      return exerciseTitle!;
    }
    if (title != null && title!.trim().isNotEmpty) {
      return title!;
    }
    return 'Progress Update';
  }

  factory VideoSubmission.fromJson(Map<String, dynamic> json) {
    return VideoSubmission(
      videoSubmissionId: json['videoSubmissionId'] as int,
      petId: json['petId'] as int,
      petName: json['petName'] as String,
      exerciseId: json['exerciseId'] as int?,
      exerciseTitle: json['exerciseTitle'] as String?,
      title: json['title'] as String?,
      notes: json['notes'] as String?,
      rawVideoStorageUrl: json['rawVideoStorageUrl'] as String,
      processedVideoStreamingUrl: json['processedVideoStreamingUrl'] as String?,
      processingStatus: json['processingStatus'] as String,
      isReviewed: json['isReviewed'] as bool,
      physioFeedbackNotes: json['physioFeedbackNotes'] as String?,
      createdDate: DateTime.parse(json['createdDate'] as String),
    );
  }
}
