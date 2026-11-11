import type { ReactNode } from 'react'
import { cn } from '@/lib/cn'
import styles from './Badge.module.css'

type BadgeVariant = 'neutral' | 'success' | 'warning' | 'error' | 'info'

interface BadgeProps {
  variant?: BadgeVariant
  dot?: boolean
  children: ReactNode
  className?: string
}

export function Badge({ variant = 'neutral', dot, children, className }: BadgeProps) {
  return (
    <span className={cn(styles.badge, styles[variant], className)}>
      {dot && <span className={styles.dot} />}
      {children}
    </span>
  )
}

interface StatusBadgeProps {
  status: string
  className?: string
}

const statusMap: Record<string, BadgeVariant> = {
  draft: 'neutral',
  pending: 'warning',
  'in-review': 'info',
  approved: 'success',
  rejected: 'error',
  active: 'success',
  inactive: 'neutral',
  open: 'info',
  closed: 'neutral',
  overdue: 'error',
  completed: 'success',
  inprogress: 'info',
  'in-progress': 'info',
  pendingapproval: 'warning',
  'pending-approval': 'warning',
  cancelled: 'neutral',
  pass: 'success',
  fail: 'error',
  conditionalpass: 'warning',
  'conditional-pass': 'warning',
  underinvestigation: 'info',
  'under-investigation': 'info',
  pendingdisposition: 'warning',
  'pending-disposition': 'warning',
  reopened: 'warning',
  minor: 'neutral',
  major: 'warning',
  critical: 'error',
  low: 'neutral',
  medium: 'warning',
  high: 'error',
  initiated: 'info',
  rcainprogress: 'info',
  actionplanning: 'warning',
  implementation: 'info',
  pendingverification: 'warning',
  closedeffective: 'success',
  closedineffective: 'error',
  underreview: 'info',
  'under-review': 'info',
  released: 'success',
  superseded: 'warning',
  obsolete: 'neutral',
  planned: 'info',
  majornonconformance: 'error',
  minornonconformance: 'warning',
  observation: 'info',
  opportunityforimprovement: 'neutral',
  internal: 'info',
  external: 'warning',
  supplier: 'neutral',
  certification: 'success',
  prospective: 'info',
  conditionalapproval: 'warning',
  'conditional-approval': 'warning',
  onprobation: 'error',
  'on-probation': 'error',
  disqualified: 'error',
  issued: 'info',
  awaitingresponse: 'warning',
  'awaiting-response': 'warning',
  accepted: 'success',
  followup: 'warning',
  'follow-up': 'warning',
  published: 'success',
  dueforcalibration: 'warning',
  'due-for-calibration': 'warning',
  incalibration: 'info',
  'in-calibration': 'info',
  outofservice: 'error',
  'out-of-service': 'error',
  retired: 'neutral',
  passwithadjustment: 'warning',
  'pass-with-adjustment': 'warning',
  limited: 'warning',
  acceptable: 'success',
  marginal: 'warning',
  unacceptable: 'error',
  // SPC statuses
  outofcontrol: 'error',
  'out-of-control': 'error',
  incontrol: 'success',
  'in-control': 'success',
  // AI Engine statuses
  timedout: 'error',
  'timed-out': 'error',
  suppressed: 'neutral',
  ignored: 'neutral',
  modified: 'info',
  shadow: 'info',
  deprecated: 'warning',
  training: 'info',
  validating: 'info',
  veryhigh: 'success',
  'very-high': 'success',
  moderate: 'warning',
  // Training & Competency statuses
  assigned: 'info',
  notstarted: 'neutral',
  'not-started': 'neutral',
  intraining: 'info',
  'in-training': 'info',
  qualified: 'success',
  expired: 'error',
  suspended: 'warning',
  revoked: 'error',
}

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const variant = statusMap[status.toLowerCase().replace(/\s+/g, '-')] ?? 'neutral'
  const label = status.charAt(0).toUpperCase() + status.slice(1)

  return (
    <Badge variant={variant} dot className={className}>
      {label}
    </Badge>
  )
}
