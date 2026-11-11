import type { ReactNode } from 'react'
import { Badge } from '@/components/ui/Badge'
import { cn } from '@/lib/cn'
import { agingVariant, humanize, sampleStatusVariant, OCCUPANCY_COLORS, OCCUPANCY_LABELS } from '../constants'
import styles from './shared.module.css'

export function PageHeader({
  title,
  subtitle,
  actions,
}: {
  title: string
  subtitle?: string
  actions?: ReactNode
}) {
  return (
    <div className={styles.header}>
      <div className={styles.headerText}>
        <h1 className={styles.title}>{title}</h1>
        {subtitle && <p className={styles.subtitle}>{subtitle}</p>}
      </div>
      {actions && <div className={styles.headerActions}>{actions}</div>}
    </div>
  )
}

type Accent = 'warning' | 'error' | 'success' | 'info' | undefined

export function StatTile({
  label,
  value,
  accent,
  hint,
  onClick,
}: {
  label: string
  value: ReactNode
  accent?: Accent
  hint?: string
  onClick?: () => void
}) {
  const accentClass =
    accent === 'warning'
      ? styles.accentWarning
      : accent === 'error'
        ? styles.accentError
        : accent === 'success'
          ? styles.accentSuccess
          : accent === 'info'
            ? styles.accentInfo
            : undefined

  const content = (
    <>
      <span className={styles.tileLabel}>{label}</span>
      <span className={cn(styles.tileValue, accentClass)}>{value}</span>
      {hint && <span className={styles.tileHint}>{hint}</span>}
    </>
  )

  return onClick ? (
    <button type="button" className={styles.tile} onClick={onClick}>
      {content}
    </button>
  ) : (
    <div className={styles.tile}>{content}</div>
  )
}

export function SampleStatusBadge({ status }: { status: string }) {
  return <Badge variant={sampleStatusVariant(status)} dot>{humanize(status)}</Badge>
}

export function AgingBadge({ category }: { category: string }) {
  return <Badge variant={agingVariant(category)} dot>{humanize(category)}</Badge>
}

export function TestResultBadge({ result }: { result: string | null }) {
  if (!result) return <span className={styles.muted}>—</span>
  return <Badge variant={result === 'Pass' ? 'success' : 'error'}>{result}</Badge>
}

/** Colored pill for a rack location's occupancy state (colors from the shared map). */
export function OccupancyPill({ state }: { state: string }) {
  const color = OCCUPANCY_COLORS[state] ?? OCCUPANCY_COLORS.Occupied
  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 6,
        fontSize: 'var(--text-sm)',
        color: 'var(--color-neutral-700)',
      }}
    >
      <span style={{ width: 10, height: 10, borderRadius: 2, background: color, flexShrink: 0 }} />
      {OCCUPANCY_LABELS[state] ?? state}
    </span>
  )
}
