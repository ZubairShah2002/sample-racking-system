import { useSearchParams, useNavigate } from 'react-router-dom'
import { useAgingReport } from '@/features/sampleTracking/api'
import { PageHeader, AgingBadge } from '@/features/sampleTracking/components/common'
import { DataTable } from '@/components/ui/DataTable'
import { Button } from '@/components/ui/Button'
import { AGING_CATEGORIES, humanize, formatDate } from '@/features/sampleTracking/constants'
import shared from '@/features/sampleTracking/components/shared.module.css'

export function SampleAgingPage() {
  const [params, setParams] = useSearchParams()
  const navigate = useNavigate()
  const category = params.get('category') ?? ''
  const { data, isLoading, error } = useAgingReport(category || undefined)

  const columns = [
    { key: 'sampleCode', header: 'Sample ID', render: (r: Record<string, unknown>) => (
      <button className={shared.linkButton} onClick={() => navigate(`/sample-tracking/samples/${r.id}`)}><span className={shared.mono}>{r.sampleCode as string}</span></button>
    ) },
    { key: 'sampleName', header: 'Name' },
    { key: 'rackLocationCode', header: 'Location', render: (r: Record<string, unknown>) => (r.rackLocationCode as string) ?? '—' },
    { key: 'receivedDate', header: 'Received', render: (r: Record<string, unknown>) => formatDate(r.receivedDate as string) },
    { key: 'totalDaysStored', header: 'Days Stored', render: (r: Record<string, unknown>) => String(r.totalDaysStored) },
    { key: 'daysOverdue', header: 'Days Overdue', render: (r: Record<string, unknown>) => (r.daysOverdue as number) > 0 ? String(r.daysOverdue) : '—' },
    { key: 'agingCategory', header: 'Aging', render: (r: Record<string, unknown>) => <AgingBadge category={r.agingCategory as string} /> },
  ]

  return (
    <div className={shared.page}>
      <PageHeader title="Sample Aging" subtitle="Storage duration monitored from each sample's original received date" />
      <div className={shared.toolbar}>
        <Button variant={category === '' ? 'primary' : 'secondary'} size="sm" onClick={() => setParams({})}>All</Button>
        {AGING_CATEGORIES.map((c) => (
          <Button key={c} size="sm" variant={category === c ? 'primary' : 'secondary'} onClick={() => setParams({ category: c })}>{humanize(c)}</Button>
        ))}
      </div>
      {error && <div className={shared.errorBanner}>Failed to load aging report.</div>}
      <DataTable
        columns={columns}
        data={(data ?? []) as unknown as Record<string, unknown>[]}
        keyExtractor={(r) => r.id as string}
        emptyTitle={isLoading ? 'Loading…' : 'No samples in this category'}
        emptyDescription="Aging is computed live from the received date."
      />
    </div>
  )
}
