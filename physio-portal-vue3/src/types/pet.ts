export interface MedicalHistory {
  medicalHistoryId: number
  diagnosis: string
  injuryOrCondition: string | null
  surgeryDate: string | null
  clinicianNotes: string | null
}

export interface Pet {
  petId: number
  ownerId: number
  ownerName: string
  petName: string
  species: string
  breed: string | null
  birthDate: string | null
  weightKg: number | null
  medicalHistories: MedicalHistory[]
}

export interface CreateMedicalHistory {
  diagnosis: string
  injuryOrCondition?: string
  surgeryDate?: string
  clinicianNotes?: string
}

export interface CreateOwner {
  email: string
  firstName: string
  lastName: string
  phoneNumber?: string
  temporaryPassword: string
}

export interface CreatePetRequest {
  ownerId?: number
  petName: string
  species: string
  breed?: string
  birthDate?: string
  weightKg?: number
  initialMedicalHistory?: CreateMedicalHistory
  newOwner?: CreateOwner
}

export interface UpdatePetRequest {
  petName: string
  species: string
  breed?: string
  birthDate?: string
  weightKg?: number
}

export const PET_SPECIES = ['Canine', 'Feline', 'Equine', 'Avian', 'Other'] as const
