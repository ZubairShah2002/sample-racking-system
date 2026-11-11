import { useRef, useState } from 'react'
import { Upload } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { apiErrorMessage, useRackLocations } from '@/features/sampleTracking/api'
import { parseWorkbook } from '@/features/sampleTracking/exportData'
import { Button } from '@/components/ui/Button'
import { Badge } from '@/components/ui/Badge'
import { toast } from '@/components/feedback/Toast'
import shared from '@/features/sampleTracking/components/shared.module.css'

interface PreviewRow {
  index: number
  sampleName: string
  itemCode: string
  supplierName: string
  locationCode: string
  receivedDate: string
  quantity: string
  errors: string[]
}

function pick(row: Record<string, unknown>, keys: string[]): string {
  for (const k of Object.keys(row)) {
    if (keys.includes(k.trim().toLowerCase())) return String(row[k] ?? '').trim()
  }
  return ''
}

export function BulkImport() {
  const { data: locations } = useRackLocations('', false)
  const fileRef = useRef<HTMLInputElement>(null)
  const [rows, setRows] = useState<PreviewRow[] | null>(null)
  const [importing, setImporting] = useState(false)
  const [result, setResult] = useState<{ ok: number; failed: number } | null>(null)

  const activeCodes = new Map((locations ?? []).filter((l) => l.isActive).map((l) => [l.code.toUpperCase(), l.id]))

  async function onFile(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return
    setResult(null)
    try {
      const raw = await parseWorkbook(file)
      const parsed: PreviewRow[] = raw.map((r, i) => {
        const sampleName = pick(r, ['samplename', 'name', 'sample name'])
        const locationCode = pick(r, ['locationcode', 'location', 'rack location', 'location code']).toUpperCase()
        const receivedDate = pick(r, ['receiveddate', 'received date', 'received'])
        const quantity = pick(r, ['quantity', 'receivedquantity', 'received quantity', 'qty']) || '1'
        const errors: string[] = []
        if (!sampleName) errors.push('Missing sample name')
        if (!locationCode) errors.push('Missing location')
        else if (!activeCodes.has(locationCode)) errors.push('Unknown/inactive location')
        const d = receivedDate ? new Date(receivedDate) : null
        if (!d || Number.isNaN(d.getTime())) errors.push('Invalid received date')
        if (!(Number(quantity) > 0)) errors.push('Invalid quantity')
        return {
          index: i + 1,
          sampleName,
          itemCode: pick(r, ['itemcode', 'item code', 'part number', 'partnumber']),
          supplierName: pick(r, ['suppliername', 'supplier', 'supplier name']),
          locationCode,
          receivedDate,
          quantity,
          errors,
        }
      })
      setRows(parsed)
    } catch (err) {
      toast({ type: 'error', title: 'Could not read file', message: apiErrorMessage(err) })
    } finally {
      e.target.value = ''
    }
  }

  async function confirmImport() {
    if (!rows) return
    const valid = rows.filter((r) => r.errors.length === 0)
    setImporting(true)
    let ok = 0
    let failed = 0
    for (const r of valid) {
      try {
        await apiClient.post('/samples', {
          rackLocationId: activeCodes.get(r.locationCode),
          sampleName: r.sampleName,
          itemCode: r.itemCode || null,
          supplierName: r.supplierName || null,
          receivedDate: new Date(r.receivedDate).toISOString().slice(0, 10),
          receivedQuantity: Number(r.quantity) || 1,
        })
        ok++
      } catch {
        failed++
      }
    }
    setImporting(false)
    setResult({ ok, failed })
    setRows(null)
    toast({ type: ok > 0 ? 'success' : 'error', title: 'Import complete', message: `${ok} imported, ${failed} failed` })
  }

  const validCount = rows?.filter((r) => r.errors.length === 0).length ?? 0
  const invalidCount = rows?.filter((r) => r.errors.length > 0).length ?? 0

  return (
    <div className={shared.formSection}>
      <h2 className={shared.sectionTitle}>Bulk Excel Import</h2>
      <p className={shared.muted} style={{ fontSize: 'var(--text-sm)', marginTop: 0 }}>
        Upload an .xlsx/.csv with columns: Sample Name, Item Code, Supplier, Location (e.g. A1-02-03), Received Date, Quantity.
        Invalid rows are flagged and never imported silently.
      </p>

      <input ref={fileRef} type="file" accept=".xlsx,.xls,.csv" onChange={onFile} style={{ display: 'none' }} />
      <Button variant="secondary" onClick={() => fileRef.current?.click()}>
        <Upload size={15} /> Choose file
      </Button>

      {result && (
        <div className={shared.infoBanner} style={{ marginTop: 12 }}>
          Imported {result.ok} sample(s){result.failed ? `, ${result.failed} failed` : ''}.
        </div>
      )}

      {rows && (
        <div style={{ marginTop: 16 }}>
          <div style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 8 }}>
            <Badge variant="success">{validCount} valid</Badge>
            {invalidCount > 0 && <Badge variant="error">{invalidCount} with errors</Badge>}
            <div style={{ marginLeft: 'auto', display: 'flex', gap: 8 }}>
              <Button variant="secondary" onClick={() => setRows(null)}>Cancel</Button>
              <Button disabled={validCount === 0 || importing} onClick={confirmImport}>
                {importing ? 'Importing…' : `Import ${validCount} valid`}
              </Button>
            </div>
          </div>
          <div style={{ maxHeight: 320, overflow: 'auto', border: '1px solid var(--color-border)', borderRadius: 'var(--radius-md)' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 'var(--text-sm)' }}>
              <thead>
                <tr style={{ textAlign: 'left', background: 'var(--color-surface-raised)' }}>
                  <th style={th}>#</th><th style={th}>Name</th><th style={th}>Item</th><th style={th}>Location</th><th style={th}>Received</th><th style={th}>Qty</th><th style={th}>Validation</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r) => (
                  <tr key={r.index} style={{ borderTop: '1px solid var(--color-border)' }}>
                    <td style={td}>{r.index}</td>
                    <td style={td}>{r.sampleName || '—'}</td>
                    <td style={td}>{r.itemCode || '—'}</td>
                    <td style={td}>{r.locationCode || '—'}</td>
                    <td style={td}>{r.receivedDate || '—'}</td>
                    <td style={td}>{r.quantity}</td>
                    <td style={td}>{r.errors.length === 0 ? <Badge variant="success">OK</Badge> : <span style={{ color: 'var(--color-error)' }}>{r.errors.join(', ')}</span>}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}

const th: React.CSSProperties = { padding: '6px 10px', fontWeight: 600, color: 'var(--color-neutral-600)' }
const td: React.CSSProperties = { padding: '6px 10px', color: 'var(--color-neutral-800)' }
