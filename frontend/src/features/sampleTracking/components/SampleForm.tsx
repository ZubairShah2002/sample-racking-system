import { useState } from 'react'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { todayInput } from '../constants'
import type { RackLocation, SampleDetail } from '../types'
import styles from './shared.module.css'

export interface SampleFormValues {
  rackLocationId: string
  sampleName: string
  itemCode: string
  productCategory: string
  model: string
  size: string
  variant: string
  materialType: string
  supplierName: string
  supplierCode: string
  purchaseOrderNumber: string
  grnNumber: string
  supplierReference: string
  receivedDate: string
  receivedTime: string
  receivedQuantity: string
  receivedBy: string
  department: string
  sampleOwner: string
  description: string
  purpose: string
  testingRequirement: string
  requiredTestingType: string
  storageRemarks: string
}

function fromDetail(d?: SampleDetail): SampleFormValues {
  return {
    rackLocationId: d?.rackLocationId ?? '',
    sampleName: d?.sampleName ?? '',
    itemCode: d?.itemCode ?? '',
    productCategory: d?.productCategory ?? '',
    model: d?.model ?? '',
    size: d?.size ?? '',
    variant: d?.variant ?? '',
    materialType: d?.materialType ?? '',
    supplierName: d?.supplierName ?? '',
    supplierCode: d?.supplierCode ?? '',
    purchaseOrderNumber: d?.purchaseOrderNumber ?? '',
    grnNumber: d?.grnNumber ?? '',
    supplierReference: d?.supplierReference ?? '',
    receivedDate: d?.receivedDate?.slice(0, 10) ?? todayInput(),
    receivedTime: d?.receivedTime?.slice(0, 5) ?? '',
    receivedQuantity: d ? String(d.receivedQuantity) : '1',
    receivedBy: d?.receivedBy ?? '',
    department: d?.department ?? '',
    sampleOwner: d?.sampleOwner ?? '',
    description: d?.description ?? '',
    purpose: d?.purpose ?? '',
    testingRequirement: d?.testingRequirement ?? '',
    requiredTestingType: d?.requiredTestingType ?? '',
    storageRemarks: d?.storageRemarks ?? '',
  }
}

export function buildSampleBody(v: SampleFormValues, includeLocation: boolean): Record<string, unknown> {
  const body: Record<string, unknown> = {
    sampleName: v.sampleName.trim(),
    itemCode: v.itemCode || null,
    productCategory: v.productCategory || null,
    model: v.model || null,
    size: v.size || null,
    variant: v.variant || null,
    materialType: v.materialType || null,
    supplierName: v.supplierName || null,
    supplierCode: v.supplierCode || null,
    purchaseOrderNumber: v.purchaseOrderNumber || null,
    grnNumber: v.grnNumber || null,
    supplierReference: v.supplierReference || null,
    receivedDate: v.receivedDate,
    receivedTime: v.receivedTime ? `${v.receivedTime}:00` : null,
    receivedQuantity: Number(v.receivedQuantity) || 1,
    receivedBy: v.receivedBy || null,
    department: v.department || null,
    sampleOwner: v.sampleOwner || null,
    description: v.description || null,
    purpose: v.purpose || null,
    testingRequirement: v.testingRequirement || null,
    requiredTestingType: v.requiredTestingType || null,
    storageRemarks: v.storageRemarks || null,
  }
  if (includeLocation) body.rackLocationId = v.rackLocationId
  return body
}

export function SampleForm({
  mode,
  initial,
  locations,
  submitting,
  onSubmit,
  onCancel,
}: {
  mode: 'create' | 'edit'
  initial?: SampleDetail
  locations?: RackLocation[]
  submitting: boolean
  onSubmit: (body: Record<string, unknown>) => void
  onCancel: () => void
}) {
  const [v, setV] = useState<SampleFormValues>(() => fromDetail(initial))
  const [touched, setTouched] = useState(false)

  const set = (k: keyof SampleFormValues) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
    setV((prev) => ({ ...prev, [k]: e.target.value }))

  const locationOptions = [
    { value: '', label: 'Select a location…' },
    ...(locations ?? []).filter((l) => l.isActive).map((l) => ({ value: l.id, label: `${l.code}${l.sampleCount ? ` (${l.sampleCount})` : ''}` })),
  ]

  const nameError = touched && !v.sampleName.trim() ? 'Sample name is required.' : undefined
  const locError = touched && mode === 'create' && !v.rackLocationId ? 'Rack location is required.' : undefined
  const qtyError = touched && !(Number(v.receivedQuantity) > 0) ? 'Quantity must be greater than zero.' : undefined

  function submit(e: React.FormEvent) {
    e.preventDefault()
    setTouched(true)
    if (!v.sampleName.trim() || !(Number(v.receivedQuantity) > 0)) return
    if (mode === 'create' && !v.rackLocationId) return
    onSubmit(buildSampleBody(v, mode === 'create'))
  }

  return (
    <form className={styles.form} onSubmit={submit}>
      <div className={styles.formSection}>
        <h2 className={styles.sectionTitle}>Identification</h2>
        <div className={styles.formGrid}>
          <Input label="Sample Name" required value={v.sampleName} onChange={set('sampleName')} error={nameError} />
          <Input label="Item Code / Part Number" value={v.itemCode} onChange={set('itemCode')} />
          <Input label="Product Category" value={v.productCategory} onChange={set('productCategory')} />
          <Input label="Model" value={v.model} onChange={set('model')} />
          <Input label="Size" value={v.size} onChange={set('size')} />
          <Input label="Variant" value={v.variant} onChange={set('variant')} />
          <Input label="Material Type" value={v.materialType} onChange={set('materialType')} />
        </div>
      </div>

      <div className={styles.formSection}>
        <h2 className={styles.sectionTitle}>Supplier Information</h2>
        <div className={styles.formGrid}>
          <Input label="Supplier Name" value={v.supplierName} onChange={set('supplierName')} />
          <Input label="Supplier Code" value={v.supplierCode} onChange={set('supplierCode')} />
          <Input label="Purchase Order Number" value={v.purchaseOrderNumber} onChange={set('purchaseOrderNumber')} />
          <Input label="GRN Number" value={v.grnNumber} onChange={set('grnNumber')} />
          <Input label="Supplier Reference" value={v.supplierReference} onChange={set('supplierReference')} />
        </div>
      </div>

      <div className={styles.formSection}>
        <h2 className={styles.sectionTitle}>Receiving Information</h2>
        <div className={styles.formGrid}>
          <Input label="Received Date" type="date" required value={v.receivedDate} onChange={set('receivedDate')} />
          <Input label="Received Time" type="time" value={v.receivedTime} onChange={set('receivedTime')} />
          <Input label="Received Quantity" type="number" min="0" step="any" required value={v.receivedQuantity} onChange={set('receivedQuantity')} error={qtyError} />
          <Input label="Received By" value={v.receivedBy} onChange={set('receivedBy')} />
          <Input label="Department" value={v.department} onChange={set('department')} />
          <Input label="Sample Owner" value={v.sampleOwner} onChange={set('sampleOwner')} />
        </div>
      </div>

      <div className={styles.formSection}>
        <h2 className={styles.sectionTitle}>Sample Information</h2>
        <div className={styles.formGrid}>
          {mode === 'create' && (
            <Select label="Rack Location" required options={locationOptions} value={v.rackLocationId} onChange={set('rackLocationId')} error={locError} />
          )}
          <Input label="Required Testing Type" value={v.requiredTestingType} onChange={set('requiredTestingType')} />
          <Input label="Testing Requirement" value={v.testingRequirement} onChange={set('testingRequirement')} />
          <Input label="Sample Purpose" value={v.purpose} onChange={set('purpose')} />
        </div>
        <div className={styles.formGrid} style={{ marginTop: 'var(--space-4)' }}>
          <div className={styles.field} style={{ gridColumn: '1 / -1' }}>
            <label className={styles.label}>Sample Description</label>
            <textarea className={styles.textarea} value={v.description} onChange={set('description')} />
          </div>
          <div className={styles.field} style={{ gridColumn: '1 / -1' }}>
            <label className={styles.label}>Storage Remarks</label>
            <textarea className={styles.textarea} value={v.storageRemarks} onChange={set('storageRemarks')} />
          </div>
        </div>
      </div>

      <div className={styles.formActions}>
        <Button type="button" variant="secondary" onClick={onCancel} disabled={submitting}>Cancel</Button>
        <Button type="submit" disabled={submitting}>
          {submitting ? 'Saving…' : mode === 'create' ? 'Register Sample' : 'Save Changes'}
        </Button>
      </div>
    </form>
  )
}
