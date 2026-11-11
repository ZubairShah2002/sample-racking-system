import { useNavigate } from 'react-router-dom'
import { apiErrorMessage, useRackLocations, useRegisterSample } from '@/features/sampleTracking/api'
import { PageHeader } from '@/features/sampleTracking/components/common'
import { SampleForm } from '@/features/sampleTracking/components/SampleForm'
import { toast } from '@/components/feedback/Toast'
import styles from '@/features/sampleTracking/components/shared.module.css'

export function SampleRegisterPage() {
  const navigate = useNavigate()
  const { data: locations } = useRackLocations('', false)
  const register = useRegisterSample()

  return (
    <div className={styles.page}>
      <PageHeader title="Register New Sample" subtitle="Record a sample into QC storage with a unique sample ID" />
      <SampleForm
        mode="create"
        locations={locations}
        submitting={register.isPending}
        onCancel={() => navigate('/sample-tracking/inventory')}
        onSubmit={(body) =>
          register.mutate(body, {
            onSuccess: (sample) => {
              toast({ type: 'success', title: 'Sample registered', message: `Created ${sample.sampleCode}` })
              navigate(`/sample-tracking/samples/${sample.id}`)
            },
            onError: (err) => toast({ type: 'error', title: 'Registration failed', message: apiErrorMessage(err) }),
          })
        }
      />
    </div>
  )
}
