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
    if (title != null && title!.trim().isNotEmpty) {
      return title!;
    }
    if (exerciseTitle != null && exerciseTitle!.trim().isNotEmpty) {
      return exerciseTitle!;
    }
    if (notes != null && notes!.trim().isNotEmpty) {
      return notes!;
    }
    return 'Video #$videoSubmissionId';
  }

  factory VideoSubmission.fromJson(Map<String, dynamic> json) {
    return VideoSubmission(
      videoSubmissionId: (json['videoSubmissionId'] as num?)?.toInt() ?? 0,
      petId: (json['petId'] as num?)?.toInt() ?? 0,
      petName: json['petName']?.toString() ?? '',
      exerciseId: (json['exerciseId'] as num?)?.toInt(),
      exerciseTitle: json['exerciseTitle']?.toString(),
      title: json['title']?.toString(),
      notes: json['notes']?.toString(),
      rawVideoStorageUrl: json['rawVideoStorageUrl']?.toString() ?? '',
      processedVideoStreamingUrl: json['processedVideoStreamingUrl']?.toString(),
      processingStatus: json['processingStatus']?.toString() ?? 'Pending',
      isReviewed: json['isReviewed'] == true,
      physioFeedbackNotes: json['physioFeedbackNotes']?.toString(),
      createdDate: json['createdDate'] != null
          ? (DateTime.tryParse(json['createdDate'].toString()) ?? DateTime.now())
          : DateTime.now(),
    );
  }
}
