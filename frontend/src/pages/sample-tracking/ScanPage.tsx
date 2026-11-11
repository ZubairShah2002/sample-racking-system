import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { apiClient } from '@/lib/api-client'
import { apiErrorMessage } from '@/features/sampleTracking/api'
import { PageHeader } from '@/features/sampleTracking/components/common'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import type { SampleDetail } from '@/features/sampleTracking/types'
import shared from '@/features/sampleTracking/components/shared.module.css'

/** Landing page for a scanned QR/barcode: resolves a sample by code and opens it. */
export function ScanPage() {
  const { code } = useParams()
  const navigate = useNavigate()
  const [manual, setManual] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  async function resolve(value: string) {
    setBusy(true)
    setError(null)
    try {
      const res = await apiClient.get<SampleDetail>(`/samples/by-code/${encodeURIComponent(value)}`)
      navigate(`/sample-tracking/samples/${res.data.id}`, { replace: true })
    } catch (e) {
      setError(apiErrorMessage(e))
    } finally {
      setBusy(false)
    }
  }

  useEffect(() => {
    if (code) resolve(code)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [code])

  return (
    <div className={shared.page}>
      <PageHeader title="Scan Sample" subtitle="Resolve a sample by its code" />
      <div className={shared.panel} style={{ maxWidth: 420 }}>
        {code && !error && <p>Looking up <strong className={shared.mono}>{code}</strong>…</p>}
        {error && <div className={shared.errorBanner} style={{ marginBottom: 12 }}>{error}</div>}
        <div style={{ display: 'flex', gap: 8, alignItems: 'flex-end' }}>
          <div style={{ flex: 1 }}>
            <Input label="Sample Code" value={manual} onChange={(e) => setManual(e.target.value)} placeholder="SMP-2026-0001" />
          </div>
          <Button disabled={!manual || busy} onClick={() => resolve(manual.trim())}>Open</Button>
        </div>
      </div>
    </div>
  )
}
