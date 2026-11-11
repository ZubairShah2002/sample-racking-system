import { useEffect, useState } from 'react'
import QRCode from 'qrcode'

/**
 * Renders a QR code encoding a deep link to the sample detail page, so a scan
 * opens the full record. Generated client-side as a data URL.
 */
export function SampleQr({ sampleCode, size = 140 }: { sampleCode: string; size?: number }) {
  const [dataUrl, setDataUrl] = useState<string>('')

  useEffect(() => {
    const url = `${window.location.origin}/sample-tracking/scan/${encodeURIComponent(sampleCode)}`
    QRCode.toDataURL(url, { width: size, margin: 1, errorCorrectionLevel: 'M' })
      .then(setDataUrl)
      .catch(() => setDataUrl(''))
  }, [sampleCode, size])

  if (!dataUrl) return <div style={{ width: size, height: size, background: 'var(--color-neutral-100)', borderRadius: 4 }} />
  return <img src={dataUrl} width={size} height={size} alt={`QR code for ${sampleCode}`} style={{ display: 'block' }} />
}
