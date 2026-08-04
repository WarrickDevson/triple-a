class Appointment {
  const Appointment({
    required this.appointmentId,
    required this.physioId,
    required this.physioName,
    required this.ownerId,
    required this.ownerName,
    required this.petId,
    required this.petName,
    required this.scheduledDateTime,
    required this.appointmentStatus,
    this.clientNotes,
    this.clinicianNotes,
  });

  final int appointmentId;
  final int physioId;
  final String physioName;
  final int ownerId;
  final String ownerName;
  final int petId;
  final String petName;
  final DateTime scheduledDateTime;
  final String appointmentStatus;
  final String? clientNotes;
  final String? clinicianNotes;

  factory Appointment.fromJson(Map<String, dynamic> json) {
    return Appointment(
      appointmentId: json['appointmentId'] as int,
      physioId: json['physioId'] as int,
      physioName: json['physioName'] as String,
      ownerId: json['ownerId'] as int,
      ownerName: json['ownerName'] as String,
      petId: json['petId'] as int,
      petName: json['petName'] as String,
      scheduledDateTime: DateTime.parse(json['scheduledDateTime'] as String),
      appointmentStatus: json['appointmentStatus'] as String,
      clientNotes: json['clientNotes'] as String?,
      clinicianNotes: json['clinicianNotes'] as String?,
    );
  }
}
