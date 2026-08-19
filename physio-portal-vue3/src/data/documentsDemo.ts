export type DocumentCategory =
  | 'Consent'
  | 'Clinical Notes'
  | 'Imaging'
  | 'Home Programs'
  | 'Other'

export interface DocumentItem {
  id: number
  name: string
  petName: string
  ownerName: string
  category: DocumentCategory
  uploadedAt: string
  sizeKb: number
  fileUrl?: string
  fileType?: string
  fileDataUrl?: string
  contentSummary?: string
}

export const DOCUMENT_CATEGORIES: DocumentCategory[] = [
  'Consent',
  'Clinical Notes',
  'Imaging',
  'Home Programs',
  'Other',
]

export const demoDocuments: DocumentItem[] = [
  {
    id: 1,
    name: 'Treatment Consent Form',
    petName: 'Bella',
    ownerName: 'Sarah Mitchell',
    category: 'Consent',
    uploadedAt: '2026-07-20',
    sizeKb: 245,
  },
  {
    id: 2,
    name: 'Initial Assessment Notes',
    petName: 'Bella',
    ownerName: 'Sarah Mitchell',
    category: 'Clinical Notes',
    uploadedAt: '2026-07-15',
    sizeKb: 128,
  },
  {
    id: 3,
    name: 'Hip X-Ray Report',
    petName: 'Rocky',
    ownerName: 'James Cooper',
    category: 'Imaging',
    uploadedAt: '2026-07-12',
    sizeKb: 1840,
  },
  {
    id: 4,
    name: 'Home Exercise Program — Week 4',
    petName: 'Maverick',
    ownerName: 'Emma van der Berg',
    category: 'Home Programs',
    uploadedAt: '2026-07-08',
    sizeKb: 512,
  },
  {
    id: 5,
    name: 'Referral Letter — Dr. van Wyk',
    petName: 'Maverick',
    ownerName: 'Emma van der Berg',
    category: 'Clinical Notes',
    uploadedAt: '2026-06-28',
    sizeKb: 96,
  },
  {
    id: 6,
    name: 'MRI Scan Summary',
    petName: 'Rocky',
    ownerName: 'James Cooper',
    category: 'Imaging',
    uploadedAt: '2026-06-25',
    sizeKb: 3200,
  },
  {
    id: 7,
    name: 'Owner Consent — Laser Therapy',
    petName: 'Whiskers',
    ownerName: 'Lisa Patel',
    category: 'Consent',
    uploadedAt: '2026-06-20',
    sizeKb: 198,
  },
  {
    id: 8,
    name: 'Discharge Instructions',
    petName: 'Whiskers',
    ownerName: 'Lisa Patel',
    category: 'Home Programs',
    uploadedAt: '2026-06-18',
    sizeKb: 340,
  },
  {
    id: 9,
    name: 'Clinic Policy Acknowledgement',
    petName: 'Bella',
    ownerName: 'Sarah Mitchell',
    category: 'Other',
    uploadedAt: '2026-06-10',
    sizeKb: 64,
  },
  {
    id: 10,
    name: 'Progress Photos — Session 6',
    petName: 'Rocky',
    ownerName: 'James Cooper',
    category: 'Clinical Notes',
    uploadedAt: '2026-06-05',
    sizeKb: 4200,
  },
]

export function formatFileSize(kb: number) {
  if (kb >= 1024) return `${(kb / 1024).toFixed(1)} MB`
  return `${kb} KB`
}

export function categoryCount(docs: DocumentItem[], category: DocumentCategory | 'All') {
  if (category === 'All') return docs.length
  return docs.filter((d) => d.category === category).length
}
