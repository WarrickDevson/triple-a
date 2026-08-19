class OwnerSubjectiveNote {
  const OwnerSubjectiveNote({
    required this.ownerSubjectiveNoteId,
    required this.petId,
    required this.ownerId,
    required this.ownerName,
    required this.noteDate,
    required this.notes,
    this.painObserved,
    this.energyObserved,
    required this.isReviewed,
  });

  final int ownerSubjectiveNoteId;
  final int petId;
  final int ownerId;
  final String ownerName;
  final DateTime noteDate;
  final String notes;
  final int? painObserved;
  final int? energyObserved;
  final bool isReviewed;

  factory OwnerSubjectiveNote.fromJson(Map<String, dynamic> json) {
    return OwnerSubjectiveNote(
      ownerSubjectiveNoteId: json['ownerSubjectiveNoteId'] as int? ?? 0,
      petId: json['petId'] as int? ?? 0,
      ownerId: json['ownerId'] as int? ?? 0,
      ownerName: json['ownerName'] as String? ?? 'Owner',
      noteDate: json['noteDate'] != null
          ? DateTime.parse(json['noteDate'] as String)
          : DateTime.now(),
      notes: json['notes'] as String? ?? '',
      painObserved: json['painObserved'] as int?,
      energyObserved: json['energyObserved'] as int?,
      isReviewed: json['isReviewed'] as bool? ?? false,
    );
  }
}
