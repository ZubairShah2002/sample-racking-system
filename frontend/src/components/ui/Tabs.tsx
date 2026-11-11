import { type ReactNode, useState } from 'react'
import { cn } from '@/lib/cn'
import styles from './Tabs.module.css'

interface TabItem {
  key: string
  label: string
  content: ReactNode
}

interface TabsProps {
  items: TabItem[]
  defaultKey?: string
  className?: string
}

export function Tabs({ items, defaultKey, className }: TabsProps) {
  const [active, setActive] = useState(defaultKey ?? items[0]?.key ?? '')
  const activeItem = items.find((i) => i.key === active)

  return (
    <div className={className}>
      <div className={styles.tabList} role="tablist">
        {items.map((item) => (
          <button
            key={item.key}
            role="tab"
            aria-selected={active === item.key}
            className={cn(styles.tab, active === item.key && styles.tabActive)}
            onClick={() => setActive(item.key)}
          >
            {item.label}
          </button>
        ))}
      </div>
      <div className={styles.panel} role="tabpanel">
        {activeItem?.content}
      </div>
    </div>
  )
}
