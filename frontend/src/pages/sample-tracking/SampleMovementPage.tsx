import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, ArrowLeftRight } from 'lucide-react'
import { apiErrorMessage, useRackLocations, useSampleAction, useSamples } from '@/features/sampleTracking/api'
import { useSampleTrackingAccess } from '@/features/sampleTracking/useAccess'
import { PageHeader } from '@/features/sampleTracking/components/common'
import { Modal } from '@/features/sampleTracking/components/Modal'
import { DataTable } from '@/components/ui/DataTable'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { toast } from '@/components/feedback/Toast'
import type { SampleSummary } from '@/features/sampleTracking/types'
import shared from '@/features/sampleTracking/components/shared.module.css'

export function SampleMovementPage() {
  const navigate = useNavigate()
  const { canWrite } = useSampleTrackingAccess()
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [moving, setMoving] = useState<SampleSummary | null>(null)
  const { data, isLoading } = useSamples({ search, page, pageSize: 20 })

  const columns = [
    { key: 'sampleCode', header: 'Sample ID', render: (r: Record<string, unknown>) => (
      <button className={shared.linkButton} onClick={() => navigate(`/sample-tracking/samples/${r.id}`)}><span className={shared.mono}>{r.sampleCode as string}</span></button>
    ) },
    { key: 'sampleName', header: 'Name' },
    { key: 'rackLocationCode', header: 'Current Location', render: (r: Record<string, unknown>) => (r.rackLocationCode as string) ?? '—' },
    ...(canWrite ? [{ key: 'actions', header: '', render: (r: Record<string, unknown>) => (
      <Button size="sm" variant="secondary" onClick={() => setMoving(r as unknown as SampleSummary)}><ArrowLeftRight size={14} /> Move</Button>
    ) }] : []),
  ]

  return (
    <div className={shared.page}>
      <PageHeader title="Sample Movement" subtitle="Move samples between rack locations — storage aging is never reset by a move" />
      <div className={shared.toolbar}>
        <div className={shared.searchWrapper}>
          <Search size={16} className={shared.searchIcon} />
          <Input className={shared.searchInput} placeholder="Search samples…" value={search} onChange={(e) => { setSearch(e.target.value); setPage(1) }} />
        </div>
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
        emptyDescription="Search for a sample to move."
      />
      {moving && <MoveModal sample={moving} onClose={() => setMoving(null)} />}
    </div>
  )
}

function MoveModal({ sample, onClose }: { sample: SampleSummary; onClose: () => void }) {
  const action = useSampleAction(sample.id, 'move')
  const { data: locations } = useRackLocations('', false)
  const [to, setTo] = useState('')
  const [reason, setReason] = useState('')
  const options = [
    { value: '', label: 'Select destination…' },
    ...(locations ?? []).filter((l) => l.isActive && l.id !== sample.rackLocationId).map((l) => ({ value: l.id, label: `${l.code} (${l.sampleCount})` })),
  ]
  return (
    <Modal open title={`Move ${sample.sampleCode}`} onClose={onClose}>
      <p className={shared.muted}>Current location: <strong>{sample.rackLocationCode ?? '—'}</strong></p>
      <Select label="Destination Location" options={options} value={to} onChange={(e) => setTo(e.target.value)} />
      <Input label="Reason" value={reason} onChange={(e) => setReason(e.target.value)} />
      <div className={shared.formActions}>
        <Button variant="secondary" onClick={onClose}>Cancel</Button>
        <Button disabled={!to || action.isPending} onClick={() => action.mutate({ toLocationId: to, reason: reason || null }, {
          onSuccess: () => { toast({ type: 'success', title: 'Sample moved' }); onClose() },
          onError: (e) => toast({ type: 'error', title: 'Move failed', message: apiErrorMessage(e) }),
        })}>Move</Button>
      </div>
    </Modal>
  )
}
