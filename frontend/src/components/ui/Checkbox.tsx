import { type InputHTMLAttributes, forwardRef } from 'react'
import { cn } from '@/lib/cn'
import styles from './Checkbox.module.css'

interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label?: string
}

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(function Checkbox(
  { label, className, id, ...props },
  ref,
) {
  const inputId = id ?? label?.toLowerCase().replace(/\s+/g, '-')

  return (
    <label className={cn(styles.wrapper, className)} htmlFor={inputId}>
      <input ref={ref} type="checkbox" id={inputId} className={styles.input} {...props} />
      {label && <span className={styles.label}>{label}</span>}
    </label>
  )
})
