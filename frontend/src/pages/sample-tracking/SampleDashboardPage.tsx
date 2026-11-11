import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { AlertTriangle } from 'lucide-react'
import { useDashboard, useRefreshAgingAlerts } from '@/features/sampleTracking/api'
import { useSampleTrackingAccess } from '@/features/sampleTracking/useAccess'
import { PageHeader, StatTile } from '@/features/sampleTracking/components/common'
import { BarList, ChartCard, Donut, MiniBars } from '@/features/sampleTracking/components/Charts'
import { formatDate } from '@/features/sampleTracking/constants'
import type { ChartPoint } from '@/features/sampleTracking/types'
import styles from '@/features/sampleTracking/components/shared.module.css'

const AGING_COLORS: Record<string, string> = {
  New: '#9e9e9e',
  Normal: '#1e40af',
  'Approaching Limit': '#b45309',
  Overdue: '#991b1b',
}

function colored(points: ChartPoint[], colors: Record<string, string>, fallback = '#486581') {
  return points.map((p) => ({ ...p, color: colors[p.label] ?? fallback }))
}

export function SampleDashboardPage() {
  const navigate = useNavigate()
  const { data, isLoading, error } = useDashboard()
  const { canWrite } = useSampleTrackingAccess()
  const refresh = useRefreshAgingAlerts()

  // Materialize aging alerts on load (idempotent) so the notification centre
  // stays current without a scheduled job. Fire-and-forget for QC users only.
  useEffect(() => {
    if (canWrite) refresh.mutate()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canWrite])

  if (error) {
    return (
      <div className={styles.page}>
        <PageHeader title="Dashboard" />
        <div className={styles.errorBanner}>Failed to load dashboard data. Please try again.</div>
      </div>
    )
  }

  const k = data?.kpis
  const occupancyColors = { Occupied: '#486581', Available: '#9fb3c8' }
  const passFailColors = { Pass: '#2d6a4f', Fail: '#991b1b' }

  return (
    <div className={styles.page}>
      <PageHeader
        title="Sample Tracking Dashboard"
        subtitle="Live overview of QC sample storage, testing and aging"
      />

      <div className={styles.kpiGrid}>
        <StatTile label="Total Samples" value={k?.totalRegisteredSamples ?? '—'} onClick={() => navigate('/sample-tracking/inventory')} />
        <StatTile label="Currently Stored" value={k?.currentlyStoredSamples ?? '—'} />
        <StatTile label="Available Locations" value={k?.availableRackLocations ?? '—'} onClick={() => navigate('/sample-tracking/rack')} />
        <StatTile label="Occupied Locations" value={k?.occupiedRackLocations ?? '—'} onClick={() => navigate('/sample-tracking/rack')} />
        <StatTile label="Overdue (90+ days)" value={k?.overdueSamples ?? '—'} accent={k && k.overdueSamples > 0 ? 'error' : undefined} onClick={() => navigate('/sample-tracking/aging?category=Overdue')} />
        <StatTile label="Approaching Limit" value={k?.approachingLimit ?? '—'} accent={k && k.approachingLimit > 0 ? 'warning' : undefined} onClick={() => navigate('/sample-tracking/aging?category=ApproachingLimit')} />
        <StatTile label="Pending Testing" value={k?.pendingTesting ?? '—'} accent={k && k.pendingTesting > 0 ? 'warning' : undefined} onClick={() => navigate('/sample-tracking/testing')} />
        <StatTile label="Testing In Progress" value={k?.testingInProgress ?? '—'} accent="info" />
        <StatTile label="Tested — Pass" value={k?.testedPass ?? '—'} accent={k && k.testedPass > 0 ? 'success' : undefined} />
        <StatTile label="Tested — Fail" value={k?.testedFail ?? '—'} accent={k && k.testedFail > 0 ? 'error' : undefined} />
        <StatTile label="Waiting for Supplier" value={k?.waitingForSupplier ?? '—'} />
        <StatTile label="To Be Disposed" value={k?.toBeDisposed ?? '—'} onClick={() => navigate('/sample-tracking/disposal')} />
      </div>

      {data && data.overdueAlerts.length > 0 && (
        <div className={styles.panel} style={{ borderColor: 'var(--color-error)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12 }}>
            <AlertTriangle size={18} color="var(--color-error)" />
            <h2 className={styles.panelTitle} style={{ margin: 0 }}>
              Overdue Sample Alerts ({data.overdueAlerts.length})
            </h2>
          </div>
          <div className={styles.timeline}>
            {data.overdueAlerts.slice(0, 10).map((a) => (
              <div key={a.sampleId} className={styles.timelineItem} style={{ borderLeftColor: 'var(--color-error)' }}>
                <button className={styles.linkButton} onClick={() => navigate(`/sample-tracking/samples/${a.sampleId}`)}>
                  {a.sampleCode}
                </button>{' '}
                — {a.sampleName} · {a.rackLocationCode ?? 'No location'}
                <div className={styles.timelineMeta}>
                  Received {formatDate(a.receivedDate)} · stored {a.totalDaysStored} days · overdue by {a.daysOverdue} days
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {isLoading ? (
        <div className={styles.panel}>Loading charts…</div>
      ) : (
        data && (
          <div className={styles.chartGrid}>
            <ChartCard title="Sample Status Distribution">
              <BarList data={data.statusDistribution} />
            </ChartCard>
            <ChartCard title="Sample Aging Distribution">
              <Donut data={colored(data.agingDistribution, AGING_COLORS)} centerLabel="stored" />
            </ChartCard>
            <ChartCard title="Rack Occupancy">
              <Donut data={colored(data.rackOccupancy, occupancyColors)} centerLabel="locations" />
            </ChartCard>
            <ChartCard title="Testing: Pass vs Fail">
              <Donut data={colored(data.testingPassFail, passFailColors)} centerLabel="tested" />
            </ChartCard>
            <ChartCard title="Monthly Sample Receiving">
              <MiniBars data={data.monthlyReceiving} />
            </ChartCard>
            <ChartCard title="Monthly Sample Movement">
              <MiniBars data={data.monthlyMovement} />
            </ChartCard>
            <ChartCard title="Samples by Supplier">
              <BarList data={data.samplesBySupplier} />
            </ChartCard>
            <ChartCard title="Samples by Product Category">
              <BarList data={data.samplesByCategory} />
            </ChartCard>
            <ChartCard title="Samples by Department">
              <BarList data={data.samplesByDepartment} />
            </ChartCard>
          </div>
        )
      )}

      {data && data.recentlyReceived.length > 0 && (
        <div className={styles.panel}>
          <h2 className={styles.sectionTitle}>Recently Received</h2>
          <div className={styles.timeline}>
            {data.recentlyReceived.map((s) => (
              <div key={s.id} className={styles.timelineItem}>
                <button className={styles.linkButton} onClick={() => navigate(`/sample-tracking/samples/${s.id}`)}>
                  {s.sampleCode}
                </button>{' '}
                — {s.sampleName} · {s.supplierName ?? 'No supplier'} · {s.rackLocationCode ?? 'No location'}
                <div className={styles.timelineMeta}>
                  Received {formatDate(s.receivedDate)} · stored {s.totalDaysStored} days · {s.agingCategory}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
