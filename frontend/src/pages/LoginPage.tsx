import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Boxes, LogIn } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { login } from '@/features/auth/api'
import { useAuthStore } from '@/stores/auth-store'
import { toast } from '@/components/feedback/Toast'
import styles from './LoginPage.module.css'

export default function LoginPage() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((s) => s.setAuth)
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      const result = await login(username.trim(), password)
      setAuth(
        {
          id: result.user.id,
          username: result.user.username,
          email: result.user.email,
          fullName: result.user.fullName,
          roles: result.user.roles,
        },
        result.token,
      )
      toast({ type: 'success', title: `Welcome back, ${result.user.fullName}` })
      navigate('/sample-tracking/dashboard', { replace: true })
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { error?: string } } })?.response?.data?.error ??
        'Unable to sign in. Please check your credentials.'
      setError(message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        <div className={styles.brand}>
          <span className={styles.logo}>
            <Boxes size={28} />
          </span>
          <div>
            <h1 className={styles.title}>Sample Racking</h1>
            <p className={styles.subtitle}>Management System</p>
          </div>
        </div>

        <form className={styles.form} onSubmit={onSubmit}>
          <Input
            label="Username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            autoComplete="username"
            autoFocus
            required
          />
          <Input
            label="Password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="current-password"
            required
          />
          {error && <div className={styles.error}>{error}</div>}
          <Button type="submit" fullWidth disabled={submitting} icon={<LogIn size={16} />}>
            {submitting ? 'Signing in…' : 'Sign in'}
          </Button>
        </form>

        <p className={styles.hint}>
          QC sample rack management — registration, 3D rack, testing, aging &amp; disposal.
        </p>
      </div>
    </div>
  )
}
