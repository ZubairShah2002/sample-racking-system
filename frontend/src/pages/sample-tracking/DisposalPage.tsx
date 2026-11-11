import { useNavigate } from 'react-router-dom'
import { useSamples } from '@/features/sampleTracking/api'
import { useSampleTrackingAccess } from '@/features/sampleTracking/useAccess'
import { PageHeader } from '@/features/sampleTracking/components/common'
import { DataTable } from '@/components/ui/DataTable'
import { formatDate } from '@/features/sampleTracking/constants'
import shared from '@/features/sampleTracking/components/shared.module.css'

export function DisposalPage() {
  const navigate = useNavigate()
  const { isAdmin } = useSampleTrackingAccess()
  const pending = useSamples({ status: 'ToBeDisposed', pageSize: 100 })
  const disposed = useSamples({ status: 'Disposed', includeDisposed: true, pageSize: 100 })

  const codeCol = {
    key: 'sampleCode', header: 'Sample ID', render: (r: Record<string, unknown>) => (
      <button className={shared.linkButton} onClick={() => navigate(`/sample-tracking/samples/${r.id}`)}><span className={shared.mono}>{r.sampleCode as string}</span></button>
    ),
  }

  return (
    <div className={shared.page}>
      <PageHeader title="Disposal Management" subtitle={isAdmin ? 'Approve or reject disposal requests. Open a sample to action it.' : 'Disposal requests require approval from an authorized user.'} />

      <div className={shared.panel}>
        <h2 className={shared.sectionTitle}>Pending Approval ({pending.data?.totalCount ?? 0})</h2>
        <DataTable
          columns={[codeCol, { key: 'sampleName', header: 'Name' }, { key: 'rackLocationCode', header: 'Location', render: (r: Record<string, unknown>) => (r.rackLocationCode as string) ?? '—' }, { key: 'receivedDate', header: 'Received', render: (r: Record<string, unknown>) => formatDate(r.receivedDate as string) }]}
          data={(pending.data?.items ?? []) as unknown as Record<string, unknown>[]}
          keyExtractor={(r) => r.id as string}
          emptyTitle={pending.isLoading ? 'Loading…' : 'No pending disposals'}
          emptyDescription="Disposal requests awaiting approval appear here."
        />
      </div>

      <div className={shared.panel}>
        <h2 className={shared.sectionTitle}>Disposed ({disposed.data?.totalCount ?? 0})</h2>
        <DataTable
          columns={[codeCol, { key: 'sampleName', header: 'Name' }, { key: 'receivedDate', header: 'Received', render: (r: Record<string, unknown>) => formatDate(r.receivedDate as string) }]}
          data={(disposed.data?.items ?? []) as unknown as Record<string, unknown>[]}
          keyExtractor={(r) => r.id as string}
          emptyTitle={disposed.isLoading ? 'Loading…' : 'No disposed samples'}
          emptyDescription="Disposed samples retain their full history."
        />
      </div>
    </div>
  )
}
