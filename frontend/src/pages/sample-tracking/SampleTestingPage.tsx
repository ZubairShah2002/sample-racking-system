import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useSamples } from '@/features/sampleTracking/api'
import { PageHeader, SampleStatusBadge, TestResultBadge } from '@/features/sampleTracking/components/common'
import { DataTable } from '@/components/ui/DataTable'
import { Select } from '@/components/ui/Select'
import { formatDate } from '@/features/sampleTracking/constants'
import shared from '@/features/sampleTracking/components/shared.module.css'

const options = [
  { value: 'PendingTesting', label: 'Pending Testing' },
  { value: 'TestingInProgress', label: 'Testing In Progress' },
  { value: 'TestedPass', label: 'Tested — Pass' },
  { value: 'TestedFail', label: 'Tested — Fail' },
  { value: '', label: 'All samples' },
]

export function SampleTestingPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState('PendingTesting')
  const [page, setPage] = useState(1)
  const { data, isLoading } = useSamples({ status, page, pageSize: 20 })

  const columns = [
    { key: 'sampleCode', header: 'Sample ID', render: (r: Record<string, unknown>) => (
      <button className={shared.linkButton} onClick={() => navigate(`/sample-tracking/samples/${r.id}`)}><span className={shared.mono}>{r.sampleCode as string}</span></button>
    ) },
    { key: 'sampleName', header: 'Name' },
    { key: 'itemCode', header: 'Item Code', render: (r: Record<string, unknown>) => (r.itemCode as string) ?? '—' },
    { key: 'rackLocationCode', header: 'Location', render: (r: Record<string, unknown>) => (r.rackLocationCode as string) ?? '—' },
    { key: 'status', header: 'Status', render: (r: Record<string, unknown>) => <SampleStatusBadge status={r.status as string} /> },
    { key: 'latestTestResult', header: 'Latest Result', render: (r: Record<string, unknown>) => <TestResultBadge result={(r.latestTestResult as string) ?? null} /> },
    { key: 'receivedDate', header: 'Received', render: (r: Record<string, unknown>) => formatDate(r.receivedDate as string) },
  ]

  return (
    <div className={shared.page}>
      <PageHeader title="Sample Testing" subtitle="Record and review QC testing results. Open a sample to record a test." />
      <div className={shared.toolbar}>
        <Select className={shared.filterSelect} options={options} value={status} onChange={(e) => { setStatus(e.target.value); setPage(1) }} />
      </div>
      <DataTable
        columns={columns}
        data={(data?.items ?? []) as unknown as Record<string, unknown>[]}
        keyExtractor={(r) => r.id as string}
        page={page}
        pageSize={20}
        totalCount={data?.totalCount}
        onPageChange={setPage}
        emptyTitle={isLoading ? 'Loading…' : 'No samples'}
        emptyDescription="Adjust the filter to see samples."
      />
    </div>
  )
}
