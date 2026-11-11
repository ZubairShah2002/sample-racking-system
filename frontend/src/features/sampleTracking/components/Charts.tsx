import type { ReactNode } from 'react'
import type { ChartPoint } from '../types'
import styles from './Charts.module.css'

export function ChartCard({ title, children, action }: { title: string; children: ReactNode; action?: ReactNode }) {
  return (
    <div className={styles.card}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 8 }}>
        <h3 className={styles.cardTitle}>{title}</h3>
        {action}
      </div>
      {children}
    </div>
  )
}

/** Horizontal bar list — single hue, magnitude comparison with direct value labels. */
export function BarList({ data, color = 'var(--color-primary-700)' }: { data: ChartPoint[]; color?: string }) {
  if (data.length === 0) return <div className={styles.empty}>No data available.</div>
  const max = Math.max(1, ...data.map((d) => d.value))
  return (
    <div className={styles.barList}>
      {data.map((d) => (
        <div className={styles.barRow} key={d.label} title={`${d.label}: ${d.value}`}>
          <span className={styles.barLabel}>{d.label}</span>
          <span className={styles.barTrack}>
            <span
              className={styles.barFill}
              style={{ width: `${(d.value / max) * 100}%`, background: color }}
            />
          </span>
          <span className={styles.barValue}>{d.value}</span>
        </div>
      ))}
    </div>
  )
}

/** Vertical mini bars for month-over-month trends (single hue, one axis). */
export function MiniBars({ data }: { data: ChartPoint[] }) {
  if (data.length === 0) return <div className={styles.empty}>No data available.</div>
  const max = Math.max(1, ...data.map((d) => d.value))
  return (
    <div className={styles.vbars}>
      {data.map((d) => (
        <div className={styles.vbarCol} key={d.label} title={`${d.label}: ${d.value}`}>
          <span className={styles.vbarValue}>{d.value || ''}</span>
          <span className={styles.vbar} style={{ height: `${(d.value / max) * 100}%` }} />
          <span className={styles.vbarLabel}>{d.label}</span>
        </div>
      ))}
    </div>
  )
}

interface DonutSlice extends ChartPoint {
  color: string
}

/** Donut for a small fixed set of labeled categories (identity via legend + labels). */
export function Donut({ data, centerLabel }: { data: DonutSlice[]; centerLabel?: string }) {
  const total = data.reduce((s, d) => s + d.value, 0)
  const size = 132
  const stroke = 20
  const r = (size - stroke) / 2
  const cx = size / 2
  const circumference = 2 * Math.PI * r
  let offset = 0

  return (
    <div className={styles.donutWrap}>
      <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} role="img" aria-label="Distribution chart">
        <circle cx={cx} cy={cx} r={r} fill="none" stroke="var(--color-neutral-100)" strokeWidth={stroke} />
        {total > 0 &&
          data
            .filter((d) => d.value > 0)
            .map((d) => {
              const len = (d.value / total) * circumference
              const dash = `${len} ${circumference - len}`
              const el = (
                <circle
                  key={d.label}
                  cx={cx}
                  cy={cx}
                  r={r}
                  fill="none"
                  stroke={d.color}
                  strokeWidth={stroke}
                  strokeDasharray={dash}
                  strokeDashoffset={-offset}
                  transform={`rotate(-90 ${cx} ${cx})`}
                >
                  <title>{`${d.label}: ${d.value}`}</title>
                </circle>
              )
              offset += len
              return el
            })}
        <text x={cx} y={cx - 2} textAnchor="middle" className={styles.donutCenter}>
          {total}
        </text>
        <text x={cx} y={cx + 16} textAnchor="middle" className={styles.donutCenterSub}>
          {centerLabel ?? 'total'}
        </text>
      </svg>
      <div className={styles.legend}>
        {data.map((d) => (
          <div className={styles.legendRow} key={d.label}>
            <span className={styles.legendSwatch} style={{ background: d.color }} />
            <span>{d.label}</span>
            <span className={styles.legendValue}>{d.value}</span>
          </div>
        ))}
      </div>
    </div>
  )
}
