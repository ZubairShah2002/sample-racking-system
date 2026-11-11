import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus, Search } from 'lucide-react'
import { useSamples } from '@/features/sampleTracking/api'
import { useSampleTrackingAccess } from '@/features/sampleTracking/useAccess'
import { PageHeader, SampleStatusBadge, AgingBadge, TestResultBadge } from '@/features/sampleTracking/components/common'
import { DataTable } from '@/components/ui/DataTable'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { Checkbox } from '@/components/ui/Checkbox'
import { SAMPLE_STATUSES, AGING_CATEGORIES, humanize, formatDate } from '@/features/sampleTracking/constants'
import type { SampleSummary } from '@/features/sampleTracking/types'
import styles from '@/features/sampleTracking/components/shared.module.css'

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
    </div>
  )
}
