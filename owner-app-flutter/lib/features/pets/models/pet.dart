class PetMedicalHistory {
  const PetMedicalHistory({
    required this.medicalHistoryId,
    required this.diagnosis,
    this.injuryOrCondition,
    this.surgeryDate,
    this.clinicianNotes,
  });

  final int medicalHistoryId;
  final String diagnosis;
  final String? injuryOrCondition;
  final String? surgeryDate;
  final String? clinicianNotes;

  factory PetMedicalHistory.fromJson(Map<String, dynamic> json) {
    return PetMedicalHistory(
      medicalHistoryId: json['medicalHistoryId'] as int,
      diagnosis: json['diagnosis'] as String,
      injuryOrCondition: json['injuryOrCondition'] as String?,
      surgeryDate: json['surgeryDate'] as String?,
      clinicianNotes: json['clinicianNotes'] as String?,
    );
  }
}

class Pet {
  const Pet({
    required this.petId,
    required this.ownerId,
    required this.ownerName,
    required this.petName,
    required this.species,
    this.breed,
    this.birthDate,
    this.weightKg,
    required this.medicalHistories,
  });

  final int petId;
  final int ownerId;
  final String ownerName;
  final String petName;
  final String species;
  final String? breed;
  final String? birthDate;
  final double? weightKg;
  final List<PetMedicalHistory> medicalHistories;

  factory Pet.fromJson(Map<String, dynamic> json) {
    return Pet(
      petId: json['petId'] as int,
      ownerId: json['ownerId'] as int,
      ownerName: json['ownerName'] as String,
      petName: json['petName'] as String,
      species: json['species'] as String,
      breed: json['breed'] as String?,
      birthDate: json['birthDate'] as String?,
      weightKg: (json['weightKg'] as num?)?.toDouble(),
      medicalHistories: (json['medicalHistories'] as List<dynamic>? ?? [])
          .map((item) => PetMedicalHistory.fromJson(item as Map<String, dynamic>))
          .toList(),
    );
  }
}

const petSpecies = ['Canine', 'Feline', 'Equine', 'Avian', 'Other'];
