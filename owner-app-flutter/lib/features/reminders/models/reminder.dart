class Reminder {
  const Reminder({
    required this.type,
    required this.title,
    required this.body,
    required this.petId,
    required this.petName,
    this.dueAt,
    this.relatedId,
  });

  final String type;
  final String title;
  final String body;
  final int petId;
  final String petName;
  final DateTime? dueAt;
  final int? relatedId;

  factory Reminder.fromJson(Map<String, dynamic> json) {
    return Reminder(
      type: json['type'] as String,
      title: json['title'] as String,
      body: json['body'] as String,
      petId: json['petId'] as int,
      petName: json['petName'] as String,
      dueAt: json['dueAt'] != null ? DateTime.parse(json['dueAt'] as String) : null,
      relatedId: json['relatedId'] as int?,
    );
  }
}
