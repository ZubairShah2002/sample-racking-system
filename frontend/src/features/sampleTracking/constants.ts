// Shared constants, label maps and small formatting helpers for the module.

export const SAMPLE_STATUSES = [
  'Received',
  'PendingTesting',
  'TestingInProgress',
  'TestedPass',
  'TestedFail',
  'OnHold',
  'WaitingForSupplier',
  'ReferenceSample',
  'ToBeDisposed',
  'Disposed',
] as const

export const AGING_CATEGORIES = ['New', 'Normal', 'ApproachingLimit', 'Overdue'] as const

export const ATTACHMENT_CATEGORIES = [
  'SamplePhoto',
  'ProductPhoto',
  'SupplierDocument',
  'InspectionDocument',
  'Other',
] as const

type BadgeVariant = 'neutral' | 'success' | 'warning' | 'error' | 'info'

/** Human label for a PascalCase enum value: "PendingTesting" -> "Pending Testing". */
export function humanize(value: string): string {
  return value.replace(/([a-z0-9])([A-Z])/g, '$1 $2')
}

export function sampleStatusVariant(status: string): BadgeVariant {
  switch (status) {
    case 'TestedPass':
      return 'success'
    case 'TestedFail':
    case 'Disposed':
      return 'error'
    case 'PendingTesting':
    case 'ToBeDisposed':
    case 'WaitingForSupplier':
    case 'OnHold':
      return 'warning'
    case 'TestingInProgress':
    case 'ReferenceSample':
      return 'info'
    default:
      return 'neutral'
  }
}

export function agingVariant(category: string): BadgeVariant {
  switch (category) {
    case 'Overdue':
      return 'error'
    case 'ApproachingLimit':
      return 'warning'
    case 'Normal':
      return 'info'
    default:
      return 'neutral'
  }
}

/**
 * Occupancy state colours for the 3D rack and rack overview. Derived from the
 * design tokens (not hardcoded per-condition in the backend) so conditions and
 * colours stay decoupled.
 */
export const OCCUPANCY_COLORS: Record<string, string> = {
  Available: '#9fb3c8', // primary-300 (muted steel)
  Occupied: '#486581', // primary-600 (navy)
  PendingTesting: '#b45309', // warning
  TestingInProgress: '#1e40af', // info
  Overdue: '#991b1b', // error
}

export const OCCUPANCY_LABELS: Record<string, string> = {
  Available: 'Available',
  Occupied: 'Occupied',
  PendingTesting: 'Pending Testing',
  TestingInProgress: 'Testing in Progress',
  Overdue: 'Overdue (90+ days)',
}

export const OCCUPANCY_LEGEND = [
  'Available',
  'Occupied',
  'PendingTesting',
  'TestingInProgress',
  'Overdue',
] as const

export function formatDate(value: string | null | undefined): string {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
}

export function formatDateTime(value: string | null | undefined): string {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  return d.toLocaleString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

/** Formats an ISO date as yyyy-MM-dd for <input type="date">. */
export function toDateInput(value: string | null | undefined): string {
  if (!value) return ''
  return value.slice(0, 10)
}

export function todayInput(): string {
  return new Date().toISOString().slice(0, 10)
}
