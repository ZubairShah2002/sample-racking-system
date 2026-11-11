import { useEffect, useState } from 'react'
import { apiErrorMessage, useSettings, useUpdateSettings } from '@/features/sampleTracking/api'
import { useSampleTrackingAccess } from '@/features/sampleTracking/useAccess'
import { PageHeader } from '@/features/sampleTracking/components/common'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Checkbox } from '@/components/ui/Checkbox'
import { toast } from '@/components/feedback/Toast'
import shared from '@/features/sampleTracking/components/shared.module.css'

export function SampleSettingsPage() {
  const { data } = useSettings()
  const { isAdmin } = useSampleTrackingAccess()
  const update = useUpdateSettings()
  const [form, setForm] = useState<Record<string, string | boolean>>({})

  useEffect(() => {
    if (data) {
      setForm({
        companyName: data.companyName ?? '',
        departmentName: data.departmentName ?? '',
        sampleIdPrefix: data.sampleIdPrefix,
        agingNewMaxDays: String(data.agingNewMaxDays),
        agingNormalMaxDays: String(data.agingNormalMaxDays),
        agingOverdueDays: String(data.agingOverdueDays),
        dateFormat: data.dateFormat,
        timeZone: data.timeZone,
        notificationsEnabled: data.notificationsEnabled,
      })
    }
  }, [data])

  const set = (k: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((p) => ({ ...p, [k]: k === 'notificationsEnabled' ? e.target.checked : e.target.value }))

  function save() {
    update.mutate({
      companyName: (form.companyName as string) || null,
      departmentName: (form.departmentName as string) || null,
      sampleIdPrefix: form.sampleIdPrefix,
      agingNewMaxDays: Number(form.agingNewMaxDays),
      agingNormalMaxDays: Number(form.agingNormalMaxDays),
      agingOverdueDays: Number(form.agingOverdueDays),
      dateFormat: form.dateFormat,
      timeZone: form.timeZone,
      notificationsEnabled: form.notificationsEnabled,
    }, {
      onSuccess: () => toast({ type: 'success', title: 'Settings saved' }),
      onError: (e) => toast({ type: 'error', title: 'Failed', message: apiErrorMessage(e) }),
    })
  }

  if (!data) return <div className={shared.page}><PageHeader title="System Settings" /><div className={shared.panel}>Loading…</div></div>

  return (
    <div className={shared.page}>
      <PageHeader title="System Settings" subtitle="Configure the rack management & sample tracking system" />
      {!isAdmin && <div className={shared.infoBanner}>Settings are read-only for your role.</div>}

      <div className={shared.formSection}>
        <h2 className={shared.sectionTitle}>Organisation</h2>
        <div className={shared.formGrid}>
          <Input label="Company Name" value={form.companyName as string ?? ''} onChange={set('companyName')} disabled={!isAdmin} />
          <Input label="Department Name" value={form.departmentName as string ?? ''} onChange={set('departmentName')} disabled={!isAdmin} />
          <Input label="Sample ID Prefix" value={form.sampleIdPrefix as string ?? ''} onChange={set('sampleIdPrefix')} disabled={!isAdmin} />
        </div>
      </div>

      <div className={shared.formSection}>
        <h2 className={shared.sectionTitle}>Aging Thresholds (days)</h2>
        <div className={shared.formGrid}>
          <Input label="New up to" type="number" value={form.agingNewMaxDays as string ?? ''} onChange={set('agingNewMaxDays')} disabled={!isAdmin} hint="0–N days = New" />
          <Input label="Normal up to" type="number" value={form.agingNormalMaxDays as string ?? ''} onChange={set('agingNormalMaxDays')} disabled={!isAdmin} />
          <Input label="Overdue at" type="number" value={form.agingOverdueDays as string ?? ''} onChange={set('agingOverdueDays')} disabled={!isAdmin} hint="Default 90" />
        </div>
      </div>

      <div className={shared.formSection}>
        <h2 className={shared.sectionTitle}>Regional & Notifications</h2>
        <div className={shared.formGrid}>
          <Input label="Date Format" value={form.dateFormat as string ?? ''} onChange={set('dateFormat')} disabled={!isAdmin} />
          <Input label="Time Zone" value={form.timeZone as string ?? ''} onChange={set('timeZone')} disabled={!isAdmin} />
        </div>
        <div style={{ marginTop: 12 }}>
          <Checkbox label="Notifications enabled" checked={!!form.notificationsEnabled} onChange={set('notificationsEnabled')} disabled={!isAdmin} />
        </div>
      </div>

      {isAdmin && (
        <div className={shared.formActions}>
          <Button onClick={save} disabled={update.isPending}>{update.isPending ? 'Saving…' : 'Save Settings'}</Button>
        </div>
      )}
    </div>
  )
}
