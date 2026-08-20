class CustomMetricModel {
  final String name;
  final double value;
  final double minScale;
  final double maxScale;
  final String? unitOrDescriptor;

  CustomMetricModel({
    required this.name,
    required this.value,
    required this.minScale,
    required this.maxScale,
    this.unitOrDescriptor,
  });

  factory CustomMetricModel.fromJson(Map<String, dynamic> json) {
    return CustomMetricModel(
      name: json['name'] as String? ?? 'Metric',
      value: (json['value'] as num?)?.toDouble() ?? 0.0,
      minScale: (json['minScale'] as num?)?.toDouble() ?? 0.0,
      maxScale: (json['maxScale'] as num?)?.toDouble() ?? 100.0,
      unitOrDescriptor: json['unitOrDescriptor'] as String?,
    );
  }
}

class SoapNoteModel {
  final int soapNoteId;
  final int petId;
  final int physioId;
  final String physioName;
  final int? appointmentId;
  final DateTime sessionDate;
  final String subjective;
  final String objective;
  final String action;
  final String plan;
  final int? stiffnessScore;
  final int? painScore;
  final int? lamenessScore;
  final List<CustomMetricModel> customMetrics;
  final bool isSharedWithOwner;
  final DateTime? sharedAtUtc;
  final DateTime createdDate;
  final String? audioUrl;
  final String? rawTranscript;

  SoapNoteModel({
    required this.soapNoteId,
    required this.petId,
    required this.physioId,
    required this.physioName,
    this.appointmentId,
    required this.sessionDate,
    required this.subjective,
    required this.objective,
    required this.action,
    required this.plan,
    this.stiffnessScore,
    this.painScore,
    this.lamenessScore,
    this.customMetrics = const [],
    required this.isSharedWithOwner,
    this.sharedAtUtc,
    required this.createdDate,
    this.audioUrl,
    this.rawTranscript,
  });

  factory SoapNoteModel.fromJson(Map<String, dynamic> json) {
    var rawMetrics = (json['customMetrics'] ?? json['CustomMetrics']) as List<dynamic>? ?? [];
    var metricsList = rawMetrics
        .whereType<Map>()
        .map((m) => CustomMetricModel.fromJson(Map<String, dynamic>.from(m)))
        .toList();

    final id = json['soapNoteId'] ?? json['SoapNoteId'] ?? 0;
    final petIdVal = json['petId'] ?? json['PetId'] ?? 0;
    final physioIdVal = json['physioId'] ?? json['PhysioId'] ?? 0;
    final physioNameVal = json['physioName'] ?? json['PhysioName'] ?? 'Physiotherapist';
    final apptIdVal = json['appointmentId'] ?? json['AppointmentId'];
    final sessionDateStr = json['sessionDate'] ?? json['SessionDate'] ?? '';
    final subjectiveVal = json['subjective'] ?? json['Subjective'] ?? '';
    final objectiveVal = json['objective'] ?? json['Objective'] ?? '';
    final actionVal = json['action'] ?? json['Action'] ?? '';
    final planVal = json['plan'] ?? json['Plan'] ?? '';
    final isSharedVal = json['isSharedWithOwner'] ?? json['IsSharedWithOwner'] ?? false;
    final sharedAtStr = json['sharedAtUtc'] ?? json['SharedAtUtc'];
    final createdDateStr = json['createdAtUtc'] ?? json['CreatedAtUtc'] ?? json['createdDate'] ?? json['CreatedDate'] ?? '';

    return SoapNoteModel(
      soapNoteId: (id as num).toInt(),
      petId: (petIdVal as num).toInt(),
      physioId: (physioIdVal as num).toInt(),
      physioName: physioNameVal.toString(),
      appointmentId: apptIdVal != null ? (apptIdVal as num).toInt() : null,
      sessionDate: DateTime.tryParse(sessionDateStr.toString()) ?? DateTime.now(),
      subjective: subjectiveVal.toString(),
      objective: objectiveVal.toString(),
      action: actionVal.toString(),
      plan: planVal.toString(),
      stiffnessScore: (json['stiffnessScore'] ?? json['StiffnessScore'] as num?)?.toInt(),
      painScore: (json['painScore'] ?? json['PainScore'] as num?)?.toInt(),
      lamenessScore: (json['lamenessScore'] ?? json['LamenessScore'] as num?)?.toInt(),
      customMetrics: metricsList,
      isSharedWithOwner: isSharedVal == true,
      sharedAtUtc: sharedAtStr != null ? DateTime.tryParse(sharedAtStr.toString()) : null,
      createdDate: DateTime.tryParse(createdDateStr.toString()) ?? DateTime.now(),
      audioUrl: (json['audioUrl'] ?? json['AudioUrl']) as String?,
      rawTranscript: (json['rawTranscript'] ?? json['RawTranscript']) as String?,
    );
  }
}
