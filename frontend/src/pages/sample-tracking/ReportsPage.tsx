import { useState } from 'react'
import { FileSpreadsheet, FileText } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { apiErrorMessage } from '@/features/sampleTracking/api'
import { PageHeader } from '@/features/sampleTracking/components/common'
import { Button } from '@/components/ui/Button'
import { toast } from '@/components/feedback/Toast'
import { exportRows } from '@/features/sampleTracking/exportData'
import { formatDate } from '@/features/sampleTracking/constants'
import type { Dashboard, Paged, RackLocation, SampleSummary } from '@/features/sampleTracking/types'
import { BulkImport } from './BulkImport'
import shared from '@/features/sampleTracking/components/shared.module.css'

type Fmt = 'xlsx' | 'csv'

function mapSample(s: SampleSummary) {
  return {
    'Sample ID': s.sampleCode,
    Name: s.sampleName,
    'Item Code': s.itemCode ?? '',
    Supplier: s.supplierName ?? '',
    Location: s.rackLocationCode ?? '',
    Status: s.status,
    'Received Date': formatDate(s.receivedDate),
    'Days Stored': s.totalDaysStored,
    'Days Overdue': s.daysOverdue,
    Aging: s.agingCategory,
    'Test Result': s.latestTestResult ?? '',
  }
}

export function ReportsPage() {
  const [busy, setBusy] = useState<string | null>(null)

  async function run(key: string, filename: string, format: Fmt, loader: () => Promise<Record<string, unknown>[]>) {
    setBusy(key + format)
    try {
      const rows = await loader()
      if (rows.length === 0) {
        toast({ type: 'info', title: 'No data', message: 'There are no records for this report.' })
        return
      }
      exportRows(rows, filename, format)
    } catch (e) {
      toast({ type: 'error', title: 'Export failed', message: apiErrorMessage(e) })
    } finally {
      setBusy(null)
    }
  }

  const samples = async (qs: string) => {
    const res = await apiClient.get<Paged<SampleSummary>>(`/samples?${qs}`)
    return res.data.items.map(mapSample)
  }

  const reports: { key: string; title: string; desc: string; loader: () => Promise<Record<string, unknown>[]> }[] = [
    { key: 'inventory', title: 'Complete Sample Inventory', desc: 'Every registered sample.', loader: () => samples('pageSize=5000&includeDisposed=true') },
    { key: 'aging', title: 'Sample Aging Report', desc: 'All stored samples with aging.', loader: async () => (await apiClient.get<SampleSummary[]>('/sample-aging')).data.map(mapSample) },
    { key: 'overdue', title: 'Overdue Sample Report', desc: 'Samples exceeding the overdue threshold.', loader: async () => (await apiClient.get<SampleSummary[]>('/sample-aging?category=Overdue')).data.map(mapSample) },
    { key: 'pending', title: 'Pending Testing Report', desc: 'Samples awaiting testing.', loader: () => samples('status=PendingTesting&pageSize=5000') },
    { key: 'tested', title: 'Testing Result Report', desc: 'Inventory with latest test outcome.', loader: () => samples('pageSize=5000&includeDisposed=true') },
    { key: 'disposal', title: 'Sample Disposal Report', desc: 'Disposed samples (history retained).', loader: () => samples('status=Disposed&includeDisposed=true&pageSize=5000') },
    { key: 'retention', title: 'Sample Retention Report', desc: 'Stored samples by storage duration.', loader: () => samples('pageSize=5000') },
    {
      key: 'occupancy', title: 'Rack Occupancy Report', desc: 'Each location and its occupancy.',
      loader: async () => {
        const res = await apiClient.get<RackLocation[]>('/rack-locations?includeInactive=true')
        return res.data.map((l) => ({ Location: l.code, Zone: l.zone, Level: l.level, Position: l.position, Active: l.isActive ? 'Yes' : 'No', Samples: l.sampleCount, State: l.occupancyState }))
      },
    },
    {
      key: 'monthly', title: 'Monthly Sample Receiving', desc: 'Receiving counts by month.',
      loader: async () => {
        const res = await apiClient.get<Dashboard>('/sample-dashboard')
        return res.data.monthlyReceiving.map((p) => ({ Month: p.label, Received: p.value }))
      },
    },
  ]

  return (
    <div className={shared.page}>
      <PageHeader title="Reports & Analytics" subtitle="Export operational reports from live database records (Excel / CSV)" />
      <div className={shared.chartGrid}>
        {reports.map((r) => (
          <div key={r.key} className={shared.panel}>
            <h3 style={{ margin: 0, fontSize: 'var(--text-base)', fontWeight: 600 }}>{r.title}</h3>
            <p className={shared.muted} style={{ fontSize: 'var(--text-sm)', margin: '4px 0 12px' }}>{r.desc}</p>
            <div style={{ display: 'flex', gap: 8 }}>
              <Button size="sm" variant="secondary" disabled={busy === r.key + 'xlsx'} onClick={() => run(r.key, r.title, 'xlsx', r.loader)}>
                <FileSpreadsheet size={14} /> Excel
              </Button>
              <Button size="sm" variant="ghost" disabled={busy === r.key + 'csv'} onClick={() => run(r.key, r.title, 'csv', r.loader)}>
                <FileText size={14} /> CSV
              </Button>
            </div>
          </div>
        ))}
      </div>

      <BulkImport />
    </div>
  )
}
