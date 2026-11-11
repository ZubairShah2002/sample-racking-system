import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Box, Plus } from 'lucide-react'
import {
  apiErrorMessage,
  useCreateRackLocation,
  useRackLocationDetail,
  useRackOverview,
  useSetRackLocationActive,
} from '@/features/sampleTracking/api'
import { useSampleTrackingAccess } from '@/features/sampleTracking/useAccess'
import { PageHeader, SampleStatusBadge, AgingBadge } from '@/features/sampleTracking/components/common'
import { Modal } from '@/features/sampleTracking/components/Modal'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { toast } from '@/components/feedback/Toast'
import { OCCUPANCY_COLORS, OCCUPANCY_LABELS, OCCUPANCY_LEGEND, formatDate } from '@/features/sampleTracking/constants'
import type { RackOccupancy } from '@/features/sampleTracking/types'
import shared from '@/features/sampleTracking/components/shared.module.css'
import styles from './RackOverview.module.css'

export function RackOverviewPage() {
  const navigate = useNavigate()
  const { data, isLoading, error } = useRackOverview()
  const { isAdmin, canWrite } = useSampleTrackingAccess()
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [addOpen, setAddOpen] = useState(false)

  const byZone = useMemo(() => {
    const map = new Map<string, RackOccupancy[]>()
    for (const loc of data?.locations ?? []) {
      if (!map.has(loc.zone)) map.set(loc.zone, [])
      map.get(loc.zone)!.push(loc)
    }
    return map
  }, [data])

  const maxPosition = data?.maxPosition ?? 4

  return (
    <div className={shared.page}>
      <PageHeader
        title="Rack Overview"
        subtitle="Live occupancy of every physical rack location"
        actions={
          <>
            <Button variant="secondary" onClick={() => navigate('/sample-tracking/rack-3d')}><Box size={16} /> 3D View</Button>
            {isAdmin && <Button onClick={() => setAddOpen(true)}><Plus size={16} /> Add Location</Button>}
          </>
        }
      />

      {error && <div className={shared.errorBanner}>Failed to load rack overview.</div>}
      {isLoading && <div className={shared.panel}>Loading…</div>}

      {data && (
        <div className={styles.layout}>
          <div className={styles.rackPanel}>
            {[...byZone.entries()].map(([zone, locs]) => {
              const levels = [...new Set(locs.map((l) => l.level))].sort((a, b) => b - a)
              return (
                <div key={zone} style={{ marginBottom: 'var(--space-5)' }}>
                  <h3 className={styles.zoneTitle}>Rack {zone}</h3>
                  {levels.map((level) => {
                    const row = locs.filter((l) => l.level === level).sort((a, b) => a.position - b.position)
                    return (
                      <div className={styles.levelRow} key={level}>
                        <div className={styles.levelLabel}>Level {String(level).padStart(2, '0')}</div>
                        <div className={styles.cells} style={{ ['--cols' as string]: maxPosition }}>
                          {row.map((loc) => (
                            <button
                              key={loc.id}
                              className={`${styles.cell} ${selectedId === loc.id ? styles.cellSelected : ''} ${!loc.isActive ? styles.cellInactive : ''}`}
                              onClick={() => setSelectedId(loc.id)}
                            >
                              <span className={styles.cellSwatch} style={{ background: OCCUPANCY_COLORS[loc.occupancyState] }} />
                              <span className={styles.cellCode}>{loc.code}</span>
                              <span className={styles.cellMeta}>
                                {loc.sampleCount === 0 ? 'Empty' : `${loc.sampleCount} sample${loc.sampleCount > 1 ? 's' : ''}`}
                              </span>
                              <span className={styles.cellMeta}>{OCCUPANCY_LABELS[loc.occupancyState]}</span>
                            </button>
                          ))}
                        </div>
                      </div>
                    )
                  })}
                </div>
              )
            })}
            <div className={styles.legend}>
              {OCCUPANCY_LEGEND.map((s) => (
                <span className={styles.legendItem} key={s}>
                  <span className={styles.legendSwatch} style={{ background: OCCUPANCY_COLORS[s] }} />
                  {OCCUPANCY_LABELS[s]}
                </span>
              ))}
            </div>
          </div>

          <LocationDetailPanel locationId={selectedId} isAdmin={isAdmin} canWrite={canWrite} />
        </div>
      )}

      {addOpen && <AddLocationModal onClose={() => setAddOpen(false)} />}
    </div>
  )
}

function LocationDetailPanel({ locationId, isAdmin, canWrite }: { locationId: string | null; isAdmin: boolean; canWrite: boolean }) {
  const navigate = useNavigate()
  const { data, isLoading } = useRackLocationDetail(locationId ?? undefined)
  const setActive = useSetRackLocationActive(locationId ?? '')

  if (!locationId) return <div className={styles.detailPanel}><div className={styles.detailEmpty}>Select a location to view its contents.</div></div>
  if (isLoading || !data) return <div className={styles.detailPanel}>Loading…</div>

  return (
    <div className={styles.detailPanel}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h3 style={{ fontFamily: 'var(--font-mono)', margin: 0, fontSize: 'var(--text-lg)' }}>{data.code}</h3>
        {!data.isActive && <span className={shared.muted}>Inactive</span>}
      </div>
      {data.description && <p className={shared.muted} style={{ marginTop: 4 }}>{data.description}</p>}
      <div style={{ margin: '12px 0', display: 'flex', gap: 8, flexWrap: 'wrap' }}>
        <span className={shared.muted}>
          {data.sampleCount}/{data.capacity} used · {data.availableCapacity} available
          {data.availableCapacity <= 0 ? ' · Full' : ''}
        </span>
      </div>

      {data.samples.length === 0 ? (
        <p className={shared.muted}>No samples stored here.</p>
      ) : (
        <div>
          {data.samples.map((s) => (
            <div key={s.id} className={styles.sampleRow}>
              <button className={shared.linkButton} onClick={() => navigate(`/sample-tracking/samples/${s.id}`)}>
                <span className={shared.mono}>{s.sampleCode}</span>
              </button> — {s.sampleName}
              <div style={{ display: 'flex', gap: 6, marginTop: 4, flexWrap: 'wrap' }}>
                <SampleStatusBadge status={s.status} />
                <AgingBadge category={s.agingCategory} />
              </div>
              <div className={shared.muted} style={{ fontSize: 'var(--text-xs)', marginTop: 2 }}>
                {s.supplierName ?? '—'} · received {formatDate(s.receivedDate)} · {s.totalDaysStored} days
              </div>
            </div>
          ))}
        </div>
      )}

      {isAdmin && (
        <div style={{ marginTop: 16, display: 'flex', gap: 8 }}>
          {data.isActive ? (
            <Button variant="secondary" disabled={data.sampleCount > 0 || setActive.isPending}
              onClick={() => setActive.mutate(false, { onSuccess: () => toast({ type: 'success', title: 'Location deactivated' }), onError: (e) => toast({ type: 'error', title: 'Failed', message: apiErrorMessage(e) }) })}>
              Deactivate
            </Button>
          ) : (
            <Button variant="secondary" disabled={setActive.isPending}
              onClick={() => setActive.mutate(true, { onSuccess: () => toast({ type: 'success', title: 'Location activated' }), onError: (e) => toast({ type: 'error', title: 'Failed', message: apiErrorMessage(e) }) })}>
              Activate
            </Button>
          )}
        </div>
      )}
      {canWrite && data.isActive && (
        <div style={{ marginTop: 8 }}>
          <Button fullWidth onClick={() => navigate('/sample-tracking/register')}>Register sample here</Button>
        </div>
      )}
    </div>
  )
}

function AddLocationModal({ onClose }: { onClose: () => void }) {
  const create = useCreateRackLocation()
  const [code, setCode] = useState('')
  const [description, setDescription] = useState('')
  const [capacity, setCapacity] = useState('4')
  return (
    <Modal open title="Add Rack Location" onClose={onClose}>
      <p className={shared.muted}>Use the ZONE-BAY-SHELF format, e.g. A1-02-30 (bay 02, shelf 30). Bay and shelf are derived from the code.</p>
      <Input label="Location Code" required value={code} onChange={(e) => setCode(e.target.value)} placeholder="A1-02-30" />
      <Input label="Description" value={description} onChange={(e) => setDescription(e.target.value)} />
      <Input label="Capacity (max samples)" type="number" min="1" value={capacity} onChange={(e) => setCapacity(e.target.value)} />
      <div className={shared.formActions}>
        <Button variant="secondary" onClick={onClose}>Cancel</Button>
        <Button disabled={!code || create.isPending} onClick={() => create.mutate({ code, description: description || null, capacity: Number(capacity) || 4 }, {
          onSuccess: () => { toast({ type: 'success', title: 'Location added' }); onClose() },
          onError: (e) => toast({ type: 'error', title: 'Failed', message: apiErrorMessage(e) }),
        })}>Add Location</Button>
      </div>
    </Modal>
  )
}
