export type InvoiceStatus = 'Paid' | 'Outstanding' | 'Overdue'

export interface InvoiceItem {
  id: number
  invoiceNumber: string
  petName: string
  ownerName: string
  date: string
  amount: number
  status: InvoiceStatus
}

export const demoInvoices: InvoiceItem[] = [
  {
    id: 1,
    invoiceNumber: 'INV-2026-0142',
    petName: 'Bella',
    ownerName: 'Sarah Mitchell',
    date: '2026-07-28',
    amount: 850,
    status: 'Outstanding',
  },
  {
    id: 2,
    invoiceNumber: 'INV-2026-0138',
    petName: 'Maverick',
    ownerName: 'Emma van der Berg',
    date: '2026-07-22',
    amount: 1200,
    status: 'Paid',
  },
  {
    id: 3,
    invoiceNumber: 'INV-2026-0131',
    petName: 'Rocky',
    ownerName: 'James Cooper',
    date: '2026-07-15',
    amount: 650,
    status: 'Paid',
  },
  {
    id: 4,
    invoiceNumber: 'INV-2026-0125',
    petName: 'Whiskers',
    ownerName: 'Lisa Patel',
    date: '2026-07-08',
    amount: 420,
    status: 'Overdue',
  },
  {
    id: 5,
    invoiceNumber: 'INV-2026-0119',
    petName: 'Bella',
    ownerName: 'Sarah Mitchell',
    date: '2026-06-30',
    amount: 850,
    status: 'Paid',
  },
]

export const billingSummary = {
  outstandingBalance: 1270,
  paidThisMonth: 2700,
  nextPaymentDue: '2026-08-15',
}

export const planFeatures: Record<string, string[]> = {
  Basic: ['Up to 25 patients', 'Exercise library', 'Basic reporting'],
  Professional: [
    'Unlimited patients',
    'Exercise library',
    'Progress reports',
    'Owner messaging',
    'Video review',
  ],
  Enterprise: [
    'Everything in Professional',
    'Multi-clinic support',
    'Custom branding',
    'Priority support',
  ],
}

export function invoiceStatusClass(status: InvoiceStatus) {
  if (status === 'Paid') return 'status-badge status-badge--improving'
  if (status === 'Overdue') return 'status-badge status-badge--at-risk'
  return 'status-badge status-badge--stable'
}

export function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(amount)
}
