import { useState } from 'react'
import { Printer } from 'lucide-react'
import { useRackLocations, useSettings } from '@/features/sampleTracking/api'
import { PageHeader } from '@/features/sampleTracking/components/common'
import { QrImage } from '@/features/sampleTracking/components/QrImage'
import { Button } from '@/components/ui/Button'
import { Checkbox } from '@/components/ui/Checkbox'
import shared from '@/features/sampleTracking/components/shared.module.css'
import styles from './RackLabels.module.css'

export function RackLabelsPage() {
  const { data: locations } = useRackLocations('', false)
  const { data: settings } = useSettings()
  const [withQr, setWithQr] = useState(true)
  const company = settings?.companyName ?? 'Berex Tech QMS'

  return (
    <div className={shared.page}>
      <div className={styles.noPrint}>
        <PageHeader
          title="Rack Label Printing"
          subtitle="Generate printable A4 labels for rack locations"
          actions={<Button onClick={() => window.print()}><Printer size={16} /> Print</Button>}
        />
        <div className={shared.toolbar}>
          <Checkbox label="Include QR code" checked={withQr} onChange={(e) => setWithQr(e.target.checked)} />
        </div>
      </div>

      <div className={styles.printArea}>
        <div className={styles.grid}>
          {(locations ?? []).map((l) => (
            <div key={l.id} className={styles.label}>
              <div className={styles.labelText}>
                <div className={styles.labelCompany}>{company} — QC Rack</div>
                <div className={styles.labelCode}>{l.code}</div>
                <div className={styles.labelMeta}>Zone {l.zone} · Level {String(l.level).padStart(2, '0')} · Position {String(l.position).padStart(2, '0')}</div>
              </div>
              {withQr && <QrImage value={`${window.location.origin}/sample-tracking/rack`} size={96} />}
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
