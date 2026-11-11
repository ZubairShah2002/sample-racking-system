import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { ArrowLeftRight, Pencil, FlaskConical, Trash2, Upload, RefreshCcw, Download, ArrowLeft, MapPin } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import {
  apiErrorMessage,
  useRackLocations,
  useSample,
  useSampleAction,
} from '@/features/sampleTracking/api'
import { useSampleTrackingAccess } from '@/features/sampleTracking/useAccess'
import { PageHeader, SampleStatusBadge, AgingBadge, TestResultBadge } from '@/features/sampleTracking/components/common'
import { Modal } from '@/features/sampleTracking/components/Modal'
import { SampleQr } from '@/features/sampleTracking/components/SampleQr'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { Badge } from '@/components/ui/Badge'
import { toast } from '@/components/feedback/Toast'
import {
  SAMPLE_STATUSES,
  ATTACHMENT_CATEGORIES,
  humanize,
  formatDate,
  formatDateTime,
  todayInput,
} from '@/features/sampleTracking/constants'
import type { SampleDetail } from '@/features/sampleTracking/types'
import styles from '@/features/sampleTracking/components/shared.module.css'

type ActionKind = 'status' | 'move' | 'test' | 'disposal' | 'attachment' | 'qr' | null

export function SampleDetailPage() {
  const { id = '' } = useParams()
  const navigate = useNavigate()
  const qc = useQueryClient()
  const { data, isLoading, error } = useSample(id)
  const { canWrite, isAdmin } = useSampleTrackingAccess()
  const [action, setAction] = useState<ActionKind>(null)

  if (error) {
    return (
      <div className={styles.page}>
        <PageHeader title="Sample" actions={<Button variant="secondary" onClick={() => navigate('/sample-tracking/inventory')}><ArrowLeft size={16} /> Back</Button>} />
        <div className={styles.errorBanner}>Sample not found.</div>
      </div>
    )
  }
  if (isLoading || !data) {
    return (
      <div className={styles.page}>
        <PageHeader title="Sample" />
        <div className={styles.panel}>Loading…</div>
      </div>
    )
  }

  const disposed = data.status === 'Disposed'
  const pendingDisposal = data.disposal?.approvalStatus === 'Pending'

  return (
    <div className={styles.page}>
      <PageHeader
        title={data.sampleCode}
        subtitle={data.sampleName}
        actions={
          <>
            <Button variant="ghost" onClick={() => setAction('qr')}>QR Code</Button>
            {canWrite && !disposed && (
              <>
                <Button variant="secondary" onClick={() => navigate(`/sample-tracking/samples/${id}/edit`)}><Pencil size={15} /> Edit</Button>
                <Button variant="secondary" onClick={() => setAction('status')}><RefreshCcw size={15} /> Status</Button>
                <Button variant="secondary" onClick={() => setAction('move')}><ArrowLeftRight size={15} /> Move</Button>
                <Button variant="secondary" onClick={() => setAction('test')}><FlaskConical size={15} /> Test</Button>
                <Button variant="secondary" onClick={() => setAction('attachment')}><Upload size={15} /> Attach</Button>
                {!pendingDisposal && <Button variant="danger" onClick={() => setAction('disposal')}><Trash2 size={15} /> Dispose</Button>}
              </>
            )}
          </>
        }
      />

      <div style={{ display: 'flex', gap: 'var(--space-2)', alignItems: 'center', flexWrap: 'wrap' }}>
        <SampleStatusBadge status={data.status} />
        <AgingBadge category={data.aging.category} />
        <TestResultBadge result={data.latestTestResult} />
        <span className={styles.muted}>Location: <strong>{data.rackLocationCode ?? '—'}</strong></span>
        {data.rackLocationCode && (
          <Button size="sm" variant="ghost" onClick={() => navigate(`/sample-tracking/rack-3d?focus=${encodeURIComponent(data.rackLocationCode!)}`)}>
            <MapPin size={14} /> Locate in 3D
          </Button>
        )}
      </div>

      <div className={styles.detailGrid}>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
          <Section title="Identification">
            <KvCols rows={[
              ['Item Code', data.itemCode], ['Product Category', data.productCategory], ['Model', data.model],
              ['Size', data.size], ['Variant', data.variant], ['Material Type', data.materialType],
            ]} />
          </Section>
          <Section title="Supplier">
            <KvCols rows={[
              ['Supplier Name', data.supplierName], ['Supplier Code', data.supplierCode],
              ['PO Number', data.purchaseOrderNumber], ['GRN Number', data.grnNumber], ['Supplier Reference', data.supplierReference],
            ]} />
          </Section>
          <Section title="Receiving">
            <KvCols rows={[
              ['Received Date', formatDate(data.receivedDate)], ['Received Time', data.receivedTime?.slice(0, 5) ?? null],
              ['Quantity', String(data.receivedQuantity)], ['Received By', data.receivedBy],
              ['Department', data.department], ['Sample Owner', data.sampleOwner],
            ]} />
          </Section>
          {(data.description || data.purpose || data.testingRequirement || data.requiredTestingType || data.storageRemarks) && (
            <Section title="Sample Information">
              <KvCols rows={[
                ['Required Testing', data.requiredTestingType], ['Testing Requirement', data.testingRequirement], ['Purpose', data.purpose],
              ]} />
              {data.description && <div className={styles.kvRow} style={{ marginTop: 12 }}><span className={styles.kvLabel}>Description</span><span className={styles.kvValue}>{data.description}</span></div>}
              {data.storageRemarks && <div className={styles.kvRow} style={{ marginTop: 12 }}><span className={styles.kvLabel}>Storage Remarks</span><span className={styles.kvValue}>{data.storageRemarks}</span></div>}
            </Section>
          )}

          <Section title={`Testing Records (${data.testingRecords.length})`}>
            {data.testingRecords.length === 0 ? <p className={styles.muted}>No testing records.</p> : (
              <div className={styles.timeline}>
                {data.testingRecords.map((t) => (
                  <div key={t.id} className={styles.timelineItem}>
                    <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                      <TestResultBadge result={t.result} /> <strong>{t.testingType}</strong>
                      {t.testingStandard && <span className={styles.muted}>· {t.testingStandard}</span>}
                    </div>
                    {t.defectDescription && <div>{t.defectDescription}</div>}
                    {t.remarks && <div className={styles.muted}>{t.remarks}</div>}
                    <div className={styles.timelineMeta}>{formatDate(t.testingDate)} · by {t.testedBy}</div>
                  </div>
                ))}
              </div>
            )}
          </Section>

          <Section title={`Movement History (${data.movements.length})`}>
            {data.movements.length === 0 ? <p className={styles.muted}>No movements recorded.</p> : (
              <div className={styles.timeline}>
                {data.movements.map((m) => (
                  <div key={m.id} className={styles.timelineItem}>
                    <div>{m.fromLocationCode ?? '—'} → <strong>{m.toLocationCode}</strong></div>
                    {m.reason && <div className={styles.muted}>{m.reason}</div>}
                    <div className={styles.timelineMeta}>{formatDateTime(m.movedAt)} · by {m.movedBy}</div>
                  </div>
                ))}
              </div>
            )}
          </Section>

          <Section title={`Status History (${data.statusHistory.length})`}>
            <div className={styles.timeline}>
              {data.statusHistory.map((h) => (
                <div key={h.id} className={styles.timelineItem}>
                  <div>{h.previousStatus ? `${humanize(h.previousStatus)} → ` : ''}<strong>{humanize(h.newStatus)}</strong></div>
                  {h.reason && <div className={styles.muted}>{h.reason}</div>}
                  <div className={styles.timelineMeta}>{formatDateTime(h.changedAt)} · by {h.changedBy}</div>
                </div>
              ))}
            </div>
          </Section>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
          <Section title="Storage Aging">
            <KvCols rows={[
              ['Received', formatDate(data.aging.receivedDate)], ['Today', formatDate(data.aging.currentDate)],
              ['Days Stored', String(data.aging.totalDaysStored)], ['Age (months)', String(data.aging.storageAgeMonths)],
              ['Days to 90', String(data.aging.daysRemainingBeforeOverdue)], ['Days Overdue', String(data.aging.daysOverdue)],
            ]} />
            <div style={{ marginTop: 12 }}><AgingBadge category={data.aging.category} /></div>
          </Section>

          <Section title="QR Code">
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 8 }}>
              <SampleQr sampleCode={data.sampleCode} />
              <span className={styles.muted} style={{ fontSize: 'var(--text-xs)' }}>Scan to open this record</span>
            </div>
          </Section>

          <Section title={`Attachments (${data.attachments.length})`}>
            {data.attachments.length === 0 ? <p className={styles.muted}>No attachments.</p> : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
                {data.attachments.map((a) => (
                  <div key={a.id} style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 'var(--text-sm)' }}>
                    <Badge variant="neutral">{humanize(a.category)}</Badge>
                    <a href={`/api/v1/samples/${id}/attachments/${a.id}/download`} target="_blank" rel="noreferrer" className={styles.linkButton} style={{ display: 'inline-flex', alignItems: 'center', gap: 4 }}>
                      <Download size={13} /> {a.fileName}
                    </a>
                  </div>
                ))}
              </div>
            )}
          </Section>

          {data.disposal && (
            <Section title="Disposal">
              <KvCols rows={[
                ['Status', data.disposal.approvalStatus], ['Reason', data.disposal.reason],
                ['Requested By', data.disposal.requestedBy], ['Approved By', data.disposal.approvedBy],
                ['Disposal Date', data.disposal.disposalDate ? formatDate(data.disposal.disposalDate) : null],
              ]} />
              {isAdmin && pendingDisposal && <DisposalApproval id={id} onDone={() => qc.invalidateQueries({ queryKey: ['st'] })} />}
            </Section>
          )}
        </div>
      </div>

      {/* Action modals */}
      {action === 'qr' && <QrModal data={data} onClose={() => setAction(null)} />}
      {action === 'status' && <StatusModal id={id} onClose={() => setAction(null)} />}
      {action === 'move' && <MoveModal id={id} currentLocationId={data.rackLocationId} onClose={() => setAction(null)} />}
      {action === 'test' && <TestModal id={id} onClose={() => setAction(null)} />}
      {action === 'disposal' && <DisposalModal id={id} onClose={() => setAction(null)} />}
      {action === 'attachment' && <AttachmentModal id={id} onClose={() => setAction(null)} onUploaded={() => qc.invalidateQueries({ queryKey: ['st'] })} />}
    </div>
  )
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className={styles.panel}>
      <h2 className={styles.sectionTitle}>{title}</h2>
      {children}
    </div>
  )
}

function KvCols({ rows }: { rows: [string, string | null | undefined][] }) {
  return (
    <div className={styles.kvCols}>
      {rows.map(([label, value]) => (
        <div className={styles.kvRow} key={label}>
          <span className={styles.kvLabel}>{label}</span>
          <span className={styles.kvValue}>{value || '—'}</span>
        </div>
      ))}
    </div>
  )
}

// ---- Action modals ----

function QrModal({ data, onClose }: { data: SampleDetail; onClose: () => void }) {
  return (
    <Modal open title={`QR — ${data.sampleCode}`} onClose={onClose} width={320}>
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 12 }}>
        <SampleQr sampleCode={data.sampleCode} size={200} />
        <div style={{ textAlign: 'center' }}>
          <div style={{ fontFamily: 'var(--font-mono)', fontWeight: 600 }}>{data.sampleCode}</div>
          <div className={styles.muted}>{data.sampleName}</div>
        </div>
        <Button variant="secondary" onClick={() => window.print()}>Print</Button>
      </div>
    </Modal>
  )
}

function StatusModal({ id, onClose }: { id: string; onClose: () => void }) {
  const action = useSampleAction(id, 'status')
  const [status, setStatus] = useState('PendingTesting')
  const [reason, setReason] = useState('')
  const [remarks, setRemarks] = useState('')
  return (
    <Modal open title="Change Status" onClose={onClose}>
      <Select label="New Status" options={SAMPLE_STATUSES.filter((s) => s !== 'Disposed').map((s) => ({ value: s, label: humanize(s) }))} value={status} onChange={(e) => setStatus(e.target.value)} />
      <Input label="Reason" value={reason} onChange={(e) => setReason(e.target.value)} />
      <Input label="Remarks" value={remarks} onChange={(e) => setRemarks(e.target.value)} />
      <div className={styles.formActions}>
        <Button variant="secondary" onClick={onClose}>Cancel</Button>
        <Button disabled={action.isPending} onClick={() => action.mutate({ status, reason: reason || null, remarks: remarks || null }, {
          onSuccess: () => { toast({ type: 'success', title: 'Status updated' }); onClose() },
          onError: (e) => toast({ type: 'error', title: 'Failed', message: apiErrorMessage(e) }),
        })}>Save</Button>
      </div>
    </Modal>
  )
}

function MoveModal({ id, currentLocationId, onClose }: { id: string; currentLocationId: string | null; onClose: () => void }) {
  const action = useSampleAction(id, 'move')
  const { data: locations } = useRackLocations('', false)
  const [to, setTo] = useState('')
  const [reason, setReason] = useState('')
  const options = [{ value: '', label: 'Select destination…' }, ...(locations ?? []).filter((l) => l.isActive && l.id !== currentLocationId).map((l) => ({ value: l.id, label: `${l.code} (${l.sampleCount})` }))]
  return (
    <Modal open title="Move Sample" onClose={onClose}>
      <Select label="Destination Location" options={options} value={to} onChange={(e) => setTo(e.target.value)} />
      <Input label="Reason" value={reason} onChange={(e) => setReason(e.target.value)} />
      <div className={styles.formActions}>
        <Button variant="secondary" onClick={onClose}>Cancel</Button>
        <Button disabled={!to || action.isPending} onClick={() => action.mutate({ toLocationId: to, reason: reason || null }, {
          onSuccess: () => { toast({ type: 'success', title: 'Sample moved' }); onClose() },
          onError: (e) => toast({ type: 'error', title: 'Move failed', message: apiErrorMessage(e) }),
        })}>Move</Button>
      </div>
    </Modal>
  )
}

function TestModal({ id, onClose }: { id: string; onClose: () => void }) {
  const action = useSampleAction(id, 'tests')
  const [type, setType] = useState('')
  const [standard, setStandard] = useState('')
  const [date, setDate] = useState(todayInput())
  const [outcome, setOutcome] = useState('Pass')
  const [defect, setDefect] = useState('')
  const [remarks, setRemarks] = useState('')
  return (
    <Modal open title="Record Testing Result" onClose={onClose}>
      <Input label="Testing Type" required value={type} onChange={(e) => setType(e.target.value)} />
      <Input label="Testing Standard" value={standard} onChange={(e) => setStandard(e.target.value)} />
      <Input label="Testing Date" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
      <Select label="Result" options={[{ value: 'Pass', label: 'Pass' }, { value: 'Fail', label: 'Fail' }]} value={outcome} onChange={(e) => setOutcome(e.target.value)} />
      {outcome === 'Fail' && <Input label="Defect Description" value={defect} onChange={(e) => setDefect(e.target.value)} />}
      <Input label="Remarks" value={remarks} onChange={(e) => setRemarks(e.target.value)} />
      <div className={styles.formActions}>
        <Button variant="secondary" onClick={onClose}>Cancel</Button>
        <Button disabled={!type || action.isPending} onClick={() => action.mutate({
          testingType: type, testingStandard: standard || null, testingDate: `${date}T00:00:00Z`, outcome,
          defectDescription: defect || null, remarks: remarks || null,
        }, {
          onSuccess: () => { toast({ type: 'success', title: 'Test recorded' }); onClose() },
          onError: (e) => toast({ type: 'error', title: 'Failed', message: apiErrorMessage(e) }),
        })}>Save</Button>
      </div>
    </Modal>
  )
}

function DisposalModal({ id, onClose }: { id: string; onClose: () => void }) {
  const action = useSampleAction(id, 'disposal')
  const [reason, setReason] = useState('')
  const [remarks, setRemarks] = useState('')
  return (
    <Modal open title="Request Disposal" onClose={onClose}>
      <p className={styles.muted}>Disposal requires approval from an authorized user. The sample is not disposed until approved.</p>
      <Input label="Disposal Reason" required value={reason} onChange={(e) => setReason(e.target.value)} />
      <Input label="Remarks" value={remarks} onChange={(e) => setRemarks(e.target.value)} />
      <div className={styles.formActions}>
        <Button variant="secondary" onClick={onClose}>Cancel</Button>
        <Button variant="danger" disabled={!reason || action.isPending} onClick={() => action.mutate({ reason, remarks: remarks || null }, {
          onSuccess: () => { toast({ type: 'success', title: 'Disposal requested' }); onClose() },
          onError: (e) => toast({ type: 'error', title: 'Failed', message: apiErrorMessage(e) }),
        })}>Request Disposal</Button>
      </div>
    </Modal>
  )
}

function DisposalApproval({ id, onDone }: { id: string; onDone: () => void }) {
  const approve = useSampleAction(id, 'disposal/approve')
  const reject = useSampleAction(id, 'disposal/reject')
  const [remarks, setRemarks] = useState('')
  return (
    <div style={{ marginTop: 12, display: 'flex', flexDirection: 'column', gap: 8 }}>
      <Input label="Approval Remarks" value={remarks} onChange={(e) => setRemarks(e.target.value)} />
      <div style={{ display: 'flex', gap: 8 }}>
        <Button variant="danger" disabled={approve.isPending} onClick={() => approve.mutate({ disposalDate: todayInput(), remarks: remarks || null }, {
          onSuccess: () => { toast({ type: 'success', title: 'Disposal approved' }); onDone() },
          onError: (e) => toast({ type: 'error', title: 'Failed', message: apiErrorMessage(e) }),
        })}>Approve Disposal</Button>
        <Button variant="secondary" disabled={reject.isPending} onClick={() => reject.mutate({ remarks: remarks || null }, {
          onSuccess: () => { toast({ type: 'success', title: 'Disposal rejected' }); onDone() },
          onError: (e) => toast({ type: 'error', title: 'Failed', message: apiErrorMessage(e) }),
        })}>Reject</Button>
      </div>
    </div>
  )
}

function AttachmentModal({ id, onClose, onUploaded }: { id: string; onClose: () => void; onUploaded: () => void }) {
  const [file, setFile] = useState<File | null>(null)
  const [category, setCategory] = useState('Other')
  const [busy, setBusy] = useState(false)
  async function upload() {
    if (!file) return
    setBusy(true)
    try {
      const fd = new FormData()
      fd.append('file', file)
      fd.append('category', category)
      await apiClient.post(`/samples/${id}/attachments`, fd, { headers: { 'Content-Type': 'multipart/form-data' } })
      toast({ type: 'success', title: 'Attachment uploaded' })
      onUploaded()
      onClose()
    } catch (e) {
      toast({ type: 'error', title: 'Upload failed', message: apiErrorMessage(e) })
    } finally {
      setBusy(false)
    }
  }
  return (
    <Modal open title="Upload Attachment" onClose={onClose}>
      <Select label="Category" options={ATTACHMENT_CATEGORIES.map((c) => ({ value: c, label: humanize(c) }))} value={category} onChange={(e) => setCategory(e.target.value)} />
      <div className={styles.field}>
        <label className={styles.label}>File</label>
        <input type="file" onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
      </div>
      <div className={styles.formActions}>
        <Button variant="secondary" onClick={onClose}>Cancel</Button>
        <Button disabled={!file || busy} onClick={upload}>{busy ? 'Uploading…' : 'Upload'}</Button>
      </div>
    </Modal>
  )
}
