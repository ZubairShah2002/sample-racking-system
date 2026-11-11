import { useEffect, useState } from 'react'
import QRCode from 'qrcode'

/** Generic QR code image for an arbitrary string value. */
export function QrImage({ value, size = 120 }: { value: string; size?: number }) {
  const [dataUrl, setDataUrl] = useState('')
  useEffect(() => {
    QRCode.toDataURL(value, { width: size, margin: 1 }).then(setDataUrl).catch(() => setDataUrl(''))
  }, [value, size])
  if (!dataUrl) return <div style={{ width: size, height: size, background: 'var(--color-neutral-100)' }} />
  return <img src={dataUrl} width={size} height={size} alt={`QR ${value}`} style={{ display: 'block' }} />
}
