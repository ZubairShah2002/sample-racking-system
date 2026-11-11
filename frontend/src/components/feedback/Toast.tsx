import { useCallback, useEffect, useState } from 'react'
import { CheckCircle, XCircle, AlertTriangle, Info, X } from 'lucide-react'
import { cn } from '@/lib/cn'
import styles from './Toast.module.css'

type ToastType = 'success' | 'error' | 'warning' | 'info'

interface ToastItem {
  id: string
  type: ToastType
  title: string
  message?: string
  duration?: number
}

const icons: Record<ToastType, typeof CheckCircle> = {
  success: CheckCircle,
  error: XCircle,
  warning: AlertTriangle,
  info: Info,
}

let addToastFn: ((toast: Omit<ToastItem, 'id'>) => void) | null = null

export function toast(item: Omit<ToastItem, 'id'>) {
  addToastFn?.(item)
}

export function ToastContainer() {
  const [toasts, setToasts] = useState<ToastItem[]>([])

  const addToast = useCallback((item: Omit<ToastItem, 'id'>) => {
    const id = crypto.randomUUID()
    setToasts((prev) => [...prev, { ...item, id }])
  }, [])

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id))
  }, [])

  useEffect(() => {
    addToastFn = addToast
    return () => { addToastFn = null }
  }, [addToast])

  return (
    <div className={styles.container}>
      {toasts.map((t) => (
        <ToastEntry key={t.id} toast={t} onClose={removeToast} />
      ))}
    </div>
  )
}

function ToastEntry({ toast: t, onClose }: { toast: ToastItem; onClose: (id: string) => void }) {
  const Icon = icons[t.type]

  useEffect(() => {
    const timer = setTimeout(() => onClose(t.id), t.duration ?? 5000)
    return () => clearTimeout(timer)
  }, [t.id, t.duration, onClose])

  return (
    <div className={cn(styles.toast, styles[t.type])}>
      <span className={styles.icon}><Icon size={18} /></span>
      <div className={styles.body}>
        <div className={styles.title}>{t.title}</div>
        {t.message && <div className={styles.message}>{t.message}</div>}
      </div>
      <button className={styles.close} onClick={() => onClose(t.id)} aria-label="Dismiss">
        <X size={14} />
      </button>
    </div>
  )
}
