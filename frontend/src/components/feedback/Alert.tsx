import type { ReactNode } from 'react'
import { CheckCircle, XCircle, AlertTriangle, Info } from 'lucide-react'
import { cn } from '@/lib/cn'
import styles from './Alert.module.css'

type AlertVariant = 'success' | 'error' | 'warning' | 'info'

interface AlertProps {
  variant: AlertVariant
  title?: string
  children: ReactNode
  className?: string
}

const icons: Record<AlertVariant, typeof CheckCircle> = {
  success: CheckCircle,
  error: XCircle,
  warning: AlertTriangle,
  info: Info,
}

export function Alert({ variant, title, children, className }: AlertProps) {
  const Icon = icons[variant]

  return (
    <div className={cn(styles.alert, styles[variant], className)} role="alert">
      <span className={styles.icon}><Icon size={18} /></span>
      <div className={styles.body}>
        {title && <div className={styles.title}>{title}</div>}
        {children}
      </div>
    </div>
  )
}
