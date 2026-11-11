import { useNavigate, useParams } from 'react-router-dom'
import { apiErrorMessage, useSample, useUpdateSample } from '@/features/sampleTracking/api'
import { PageHeader } from '@/features/sampleTracking/components/common'
import { SampleForm } from '@/features/sampleTracking/components/SampleForm'
import { toast } from '@/components/feedback/Toast'
import styles from '@/features/sampleTracking/components/shared.module.css'

export function SampleEditPage() {
  const { id = '' } = useParams()
  const navigate = useNavigate()
  const { data, isLoading } = useSample(id)
  const update = useUpdateSample(id)

  if (isLoading || !data) {
    return (
      <div className={styles.page}>
        <PageHeader title="Edit Sample" />
        <div className={styles.panel}>Loading…</div>
      </div>
    )
  }

  return (
    <div className={styles.page}>
      <PageHeader title={`Edit ${data.sampleCode}`} subtitle="Previous values are preserved in the audit trail" />
      <SampleForm
        mode="edit"
        initial={data}
        submitting={update.isPending}
        onCancel={() => navigate(`/sample-tracking/samples/${id}`)}
        onSubmit={(body) =>
          update.mutate(body, {
            onSuccess: () => {
              toast({ type: 'success', title: 'Sample updated' })
              navigate(`/sample-tracking/samples/${id}`)
            },
            onError: (err) => toast({ type: 'error', title: 'Update failed', message: apiErrorMessage(err) }),
          })
        }
      />
    </div>
  )
}
