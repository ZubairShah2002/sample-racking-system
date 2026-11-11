import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Check, CheckCheck } from 'lucide-react'
import { useMarkAllNotificationsRead, useMarkNotification, useNotifications } from '@/features/sampleTracking/api'
import { PageHeader } from '@/features/sampleTracking/components/common'
import { Button } from '@/components/ui/Button'
import { Checkbox } from '@/components/ui/Checkbox'
import { Badge } from '@/components/ui/Badge'
import { formatDateTime, humanize } from '@/features/sampleTracking/constants'
import shared from '@/features/sampleTracking/components/shared.module.css'

const sevVariant: Record<string, 'neutral' | 'warning' | 'error' | 'info'> = {
  Info: 'info',
  Warning: 'warning',
  Critical: 'error',
}

export function NotificationsPage() {
  const navigate = useNavigate()
  const [unreadOnly, setUnreadOnly] = useState(false)
  const { data, isLoading } = useNotifications(unreadOnly, 100)
  const mark = useMarkNotification()
  const markAll = useMarkAllNotificationsRead()

  return (
    <div className={shared.page}>
      <PageHeader
        title="Notification Centre"
        subtitle={`${data?.unreadCount ?? 0} unread`}
        actions={<Button variant="secondary" onClick={() => markAll.mutate()} disabled={markAll.isPending}><CheckCheck size={16} /> Mark all read</Button>}
      />
      <div className={shared.toolbar}>
        <Checkbox label="Unread only" checked={unreadOnly} onChange={(e) => setUnreadOnly(e.target.checked)} />
      </div>

      <div className={shared.panel} style={{ padding: 0 }}>
        {isLoading && <div style={{ padding: 20 }}>Loading…</div>}
        {!isLoading && (data?.items.length ?? 0) === 0 && <div style={{ padding: 20 }} className={shared.muted}>No notifications.</div>}
        {data?.items.map((n) => (
          <div key={n.id} style={{
            display: 'flex', alignItems: 'flex-start', gap: 12, padding: '12px 20px',
            borderBottom: '1px solid var(--color-border)', background: n.isRead ? 'transparent' : 'var(--color-primary-50)',
          }}>
            <Badge variant={sevVariant[n.severity] ?? 'neutral'}>{humanize(n.type)}</Badge>
            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ fontWeight: 600, fontSize: 'var(--text-sm)' }}>{n.title}</div>
              <div className={shared.muted} style={{ fontSize: 'var(--text-sm)' }}>
                {n.sampleId ? (
                  <button className={shared.linkButton} onClick={() => navigate(`/sample-tracking/samples/${n.sampleId}`)}>{n.message}</button>
                ) : n.message}
              </div>
              <div className={shared.muted} style={{ fontSize: 'var(--text-xs)', marginTop: 2 }}>{formatDateTime(n.createdAt)}</div>
            </div>
            {!n.isRead && (
              <Button size="sm" variant="ghost" onClick={() => mark.mutate({ id: n.id, read: true })} aria-label="Mark read"><Check size={14} /></Button>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}
