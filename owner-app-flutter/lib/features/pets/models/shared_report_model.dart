class SharedReportModel {
  final int sharedReportId;
  final int petId;
  final String? petName;
  final int? soapNoteId;
  final int sharedByPhysioId;
  final String sharedByPhysioName;
  final String title;
  final String reportType;
  final String? summary;
  final DateTime sharedAtUtc;
  final String? fileUrl;
  final String? fileType;
  final int? fileSizeBytes;

  SharedReportModel({
    required this.sharedReportId,
    required this.petId,
    this.petName,
    this.soapNoteId,
    required this.sharedByPhysioId,
    required this.sharedByPhysioName,
    required this.title,
    required this.reportType,
    this.summary,
    required this.sharedAtUtc,
    this.fileUrl,
    this.fileType,
    this.fileSizeBytes,
  });

  bool get isSoapNote =>
      reportType.toUpperCase().contains('SOAP') || soapNoteId != null;

  bool get isClinicalReport =>
      reportType.toUpperCase().contains('CLINICAL') ||
      reportType.toUpperCase().contains('PROGRESS') ||
      reportType.toUpperCase().contains('DISCHARGE');

  bool get isHomeProgram =>
      reportType.toUpperCase().contains('PROGRAM') ||
      reportType.toUpperCase().contains('CARE');

  String get categoryLabel {
    if (isSoapNote) return 'SOAP Assessment';
    if (reportType.toUpperCase().contains('DISCHARGE')) return 'Discharge Summary';
    if (isClinicalReport) return 'Clinical Progress Report';
    if (isHomeProgram) return 'Home Care Plan';
    if (reportType.toUpperCase().contains('REFERRAL')) return 'Referral Letter';
    if (reportType.toUpperCase().contains('IMAGING')) return 'Imaging & Diagnostics';
    if (reportType.toUpperCase().contains('CONSENT')) return 'Consent Form';
    return 'Clinical Document';
  }

  factory SharedReportModel.fromJson(Map<String, dynamic> json) {
    final id = json['sharedReportId'] ?? json['SharedReportId'] ?? json['id'] ?? 0;
    final petIdVal = json['petId'] ?? json['PetId'] ?? 0;
    final petNameVal = json['petName'] ?? json['PetName'];
    final soapNoteIdVal = json['soapNoteId'] ?? json['SoapNoteId'];
    final physioIdVal = json['sharedByPhysioId'] ?? json['SharedByPhysioId'] ?? 0;
    final physioNameVal = json['sharedByPhysioName'] ?? json['SharedByPhysioName'] ?? 'Clinician';
    final titleVal = json['title'] ?? json['Title'] ?? 'Clinical Document';
    final reportTypeVal = json['reportType'] ?? json['ReportType'] ?? 'SOAP_SESSION';
    final summaryVal = json['summary'] ?? json['Summary'] as String?;
    final sharedAtStr = json['sharedAtUtc'] ?? json['SharedAtUtc'] ?? json['sharedAt'] ?? '';

    return SharedReportModel(
      sharedReportId: (id as num).toInt(),
      petId: (petIdVal as num).toInt(),
      petName: petNameVal?.toString(),
      soapNoteId: soapNoteIdVal != null ? (soapNoteIdVal as num).toInt() : null,
      sharedByPhysioId: (physioIdVal as num).toInt(),
      sharedByPhysioName: physioNameVal.toString(),
      title: titleVal.toString(),
      reportType: reportTypeVal.toString(),
      summary: summaryVal?.toString(),
      sharedAtUtc: DateTime.tryParse(sharedAtStr.toString()) ?? DateTime.now(),
      fileUrl: (json['fileUrl'] ?? json['FileUrl']) as String?,
      fileType: (json['fileType'] ?? json['FileType']) as String?,
      fileSizeBytes: (json['fileSizeBytes'] ?? json['FileSizeBytes']) as int?,
    );
  }
}
