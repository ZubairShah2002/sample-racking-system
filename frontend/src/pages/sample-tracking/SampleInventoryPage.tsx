import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { Plus, Search, Workflow } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { useSamples, apiErrorMessage } from '@/features/sampleTracking/api'
import { useSampleTrackingAccess } from '@/features/sampleTracking/useAccess'
import { PageHeader, SampleStatusBadge, AgingBadge, TestResultBadge } from '@/features/sampleTracking/components/common'
import { Modal } from '@/features/sampleTracking/components/Modal'
import { toast } from '@/components/feedback/Toast'
import { DataTable } from '@/components/ui/DataTable'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { Checkbox } from '@/components/ui/Checkbox'
import { SAMPLE_STATUSES, AGING_CATEGORIES, humanize, formatDate } from '@/features/sampleTracking/constants'
import type { SampleSummary } from '@/features/sampleTracking/types'
import styles from '@/features/sampleTracking/components/shared.module.css'

type StageTarget = { id: string; sampleCode: string; status: string }

const statusOptions = [{ value: '', label: 'All statuses' }, ...SAMPLE_STATUSES.map((s) => ({ value: s, label: humanize(s) }))]
const agingOptions = [{ value: '', label: 'All aging' }, ...AGING_CATEGORIES.map((s) => ({ value: s, label: humanize(s) }))]

export function SampleInventoryPage() {
  const navigate = useNavigate()
  const { canWrite } = useSampleTrackingAccess()
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [agingCategory, setAgingCategory] = useState('')
  const [includeDisposed, setIncludeDisposed] = useState(false)
  const [sortBy, setSortBy] = useState('received')
  const [sortDir, setSortDir] = useState('desc')
  const [page, setPage] = useState(1)
  const [stage, setStage] = useState<StageTarget | null>(null)
  const pageSize = 20

  const { data, isLoading, error } = useSamples({
    search, status, agingCategory, includeDisposed, sortBy, sortDir, page, pageSize,
  })

  const columns = [
    {
      key: 'sampleCode',
      header: 'Sample ID',
      sortable: true,
      render: (row: Record<string, unknown>) => (
        <button className={styles.linkButton} onClick={() => navigate(`/sample-tracking/samples/${row.id}`)}>
          <span className={styles.mono}>{row.sampleCode as string}</span>
        </button>
      ),
    },
    { key: 'sampleName', header: 'Name', sortable: true },
    { key: 'itemCode', header: 'Item Code', render: (r: Record<string, unknown>) => (r.itemCode as string) ?? '—' },
    { key: 'supplierName', header: 'Supplier', render: (r: Record<string, unknown>) => (r.supplierName as string) ?? '—' },
    { key: 'rackLocationCode', header: 'Location', render: (r: Record<string, unknown>) => (r.rackLocationCode as string) ?? '—' },
    { key: 'status', header: 'Status', render: (r: Record<string, unknown>) => <SampleStatusBadge status={r.status as string} /> },
    { key: 'receivedDate', header: 'Received', sortable: true, render: (r: Record<string, unknown>) => formatDate(r.receivedDate as string) },
    { key: 'totalDaysStored', header: 'Days', render: (r: Record<string, unknown>) => String(r.totalDaysStored ?? 0) },
    { key: 'agingCategory', header: 'Aging', render: (r: Record<string, unknown>) => <AgingBadge category={r.agingCategory as string} /> },
    { key: 'latestTestResult', header: 'Test', render: (r: Record<string, unknown>) => <TestResultBadge result={(r.latestTestResult as string) ?? null} /> },
    ...(canWrite
      ? [{
          key: 'stage',
          header: '',
          render: (r: Record<string, unknown>) =>
            (r.status as string) === 'Disposed' ? (
              <span className={styles.muted}>—</span>
            ) : (
              <Button
                size="sm"
                variant="secondary"
                icon={<Workflow size={14} />}
                onClick={() =>
                  setStage({ id: r.id as string, sampleCode: r.sampleCode as string, status: r.status as string })
                }
              >
                Stage
              </Button>
            ),
        }]
      : []),
  ]

  return (
    <div className={styles.page}>
      <PageHeader
        title="Sample Inventory"
        subtitle="Search and manage all registered samples"
        actions={
          canWrite && (
            <Button onClick={() => navigate('/sample-tracking/register')}>
              <Plus size={16} /> Register Sample
            </Button>
          )
        }
      />

      <div className={styles.toolbar}>
        <div className={styles.searchWrapper}>
          <Search size={16} className={styles.searchIcon} />
          <Input
            className={styles.searchInput}
            placeholder="Search ID, name, item code, supplier, PO, GRN…"
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1) }}
          />
        </div>
        <Select className={styles.filterSelect} options={statusOptions} value={status} onChange={(e) => { setStatus(e.target.value); setPage(1) }} />
        <Select className={styles.filterSelect} options={agingOptions} value={agingCategory} onChange={(e) => { setAgingCategory(e.target.value); setPage(1) }} />
        <Checkbox label="Include disposed" checked={includeDisposed} onChange={(e) => { setIncludeDisposed(e.target.checked); setPage(1) }} />
      </div>

      {error && <div className={styles.errorBanner}>Failed to load samples. Please try again.</div>}

      <DataTable
        columns={columns}
        data={(data?.items ?? []) as unknown as Record<string, unknown>[]}
        keyExtractor={(row) => (row as unknown as SampleSummary).id}
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount}
        onPageChange={setPage}
        onSort={(key, dir) => { setSortBy(key === 'sampleCode' ? 'code' : key === 'sampleName' ? 'name' : 'received'); setSortDir(dir); setPage(1) }}
        emptyTitle={isLoading ? 'Loading…' : 'No samples found'}
        emptyDescription="Register a sample or adjust your filters."
      />

      {stage && <StageModal target={stage} onClose={() => setStage(null)} />}
    </div>
  )
}

/** Quick stage mover: push a registered sample through its QC lifecycle. */
function StageModal({ target, onClose }: { target: StageTarget; onClose: () => void }) {
  const qc = useQueryClient()
  const [busy, setBusy] = useState<string | null>(null)

  async function go(label: string, run: () => Promise<unknown>) {
    setBusy(label)
    try {
      await run()
      qc.invalidateQueries({ queryKey: ['st'] })
      toast({ type: 'success', title: `${target.sampleCode} → ${label}` })
      onClose()
    } catch (e) {
      toast({ type: 'error', title: 'Could not change stage', message: apiErrorMessage(e) })
      setBusy(null)
    }
  }

  const setStatus = (status: string) => () => apiClient.post(`/samples/${target.id}/status`, { status })
  const requestDisposal = () => apiClient.post(`/samples/${target.id}/disposal`, { reason: 'Disposal requested from inventory' })

  const stages: { label: string; status?: string; run: () => Promise<unknown>; danger?: boolean }[] = [
    { label: 'Pending Testing', run: setStatus('PendingTesting') },
    { label: 'Under Testing', run: setStatus('TestingInProgress') },
    { label: 'Done Testing — Pass', run: setStatus('TestedPass') },
    { label: 'Done Testing — Fail', run: setStatus('TestedFail') },
    { label: 'Request Disposal', run: requestDisposal, danger: true },
  ]

  return (
    <Modal open title={`Move ${target.sampleCode} to stage`} onClose={onClose} width={440}>
      <p className={styles.muted} style={{ marginTop: 0 }}>
        Current status: <strong>{humanize(target.status)}</strong>
      </p>
      <div style={{ display: 'grid', gap: 8 }}>
        {stages.map((s) => (
          <Button
            key={s.label}
            variant={s.danger ? 'danger' : 'secondary'}
            fullWidth
            disabled={!!busy}
            onClick={() => go(s.label, s.run)}
          >
            {busy === s.label ? 'Saving…' : s.label}
          </Button>
        ))}
      </div>
      <p className={styles.muted} style={{ fontSize: '0.78rem', marginBottom: 0 }}>
        Disposal needs an authorized approval afterwards (Disposal page). Aging is never reset by a stage change.
      </p>
    </Modal>
  )
}
