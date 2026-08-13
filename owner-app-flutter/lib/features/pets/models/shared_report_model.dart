class SharedReportModel {
  final int sharedReportId;
  final int petId;
  final int? soapNoteId;
  final int sharedByPhysioId;
  final String sharedByPhysioName;
  final String title;
  final String reportType;
  final String? summary;
  final DateTime sharedAtUtc;

  SharedReportModel({
    required this.sharedReportId,
    required this.petId,
    this.soapNoteId,
    required this.sharedByPhysioId,
    required this.sharedByPhysioName,
    required this.title,
    required this.reportType,
    this.summary,
    required this.sharedAtUtc,
  });

  factory SharedReportModel.fromJson(Map<String, dynamic> json) {
    return SharedReportModel(
      sharedReportId: json['sharedReportId'] as int,
      petId: json['petId'] as int,
      soapNoteId: json['soapNoteId'] as int?,
      sharedByPhysioId: json['sharedByPhysioId'] as int? ?? 0,
      sharedByPhysioName: json['sharedByPhysioName'] as String? ?? 'Clinician',
      title: json['title'] as String? ?? 'Clinical Session Report',
      reportType: json['reportType'] as String? ?? 'SOAP_SESSION',
      summary: json['summary'] as String?,
      sharedAtUtc: DateTime.tryParse(json['sharedAtUtc'] as String? ?? '') ?? DateTime.now(),
    );
  }
}
