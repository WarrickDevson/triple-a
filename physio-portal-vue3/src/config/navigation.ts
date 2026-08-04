export interface PortalNavItem {
  name: string
  label: string
  to: { name: string }
  icon: string
  badge?: number
}

export const portalNavItems: PortalNavItem[] = [
  { name: 'dashboard', label: 'Dashboard', to: { name: 'dashboard' }, icon: 'LayoutDashboard' },
  { name: 'patients', label: 'Patients', to: { name: 'patients' }, icon: 'PawPrint' },
  { name: 'appointments', label: 'Appointments', to: { name: 'appointments' }, icon: 'Calendar' },
  { name: 'treatment-plans', label: 'Treatment Plans', to: { name: 'treatment-plans' }, icon: 'ClipboardList' },
  { name: 'exercises', label: 'Exercise Library', to: { name: 'exercises' }, icon: 'Dumbbell' },
  { name: 'progress', label: 'Progress', to: { name: 'progress' }, icon: 'TrendingUp' },
  { name: 'messages', label: 'Messages', to: { name: 'messages' }, icon: 'MessageSquare' },
  { name: 'reports', label: 'Reports', to: { name: 'reports' }, icon: 'FileBarChart' },
  { name: 'documents', label: 'Documents', to: { name: 'documents' }, icon: 'FolderOpen' },
  { name: 'tasks', label: 'Tasks', to: { name: 'tasks' }, icon: 'CheckSquare' },
  { name: 'billing', label: 'Billing', to: { name: 'billing' }, icon: 'CreditCard' },
  { name: 'settings', label: 'Settings', to: { name: 'settings' }, icon: 'Settings' },
]
