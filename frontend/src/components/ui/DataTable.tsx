import { type ReactNode, useState, useCallback } from 'react'
import { ChevronUp, ChevronDown, ChevronsUpDown, ChevronLeft, ChevronRight, Inbox } from 'lucide-react'
import { cn } from '@/lib/cn'
import styles from './DataTable.module.css'

type SortDirection = 'asc' | 'desc'

interface Column<T> {
  key: string
  header: string
  sortable?: boolean
  render?: (row: T) => ReactNode
  width?: string
}

interface DataTableProps<T> {
  columns: Column<T>[]
  data: T[]
  keyExtractor: (row: T) => string
  page?: number
  pageSize?: number
  totalCount?: number
  onPageChange?: (page: number) => void
  onSort?: (key: string, direction: SortDirection) => void
  emptyTitle?: string
  emptyDescription?: string
  className?: string
}

export function DataTable<T extends Record<string, unknown>>({
  columns,
  data,
  keyExtractor,
  page = 1,
  pageSize = 20,
  totalCount,
  onPageChange,
  onSort,
  emptyTitle = 'No data',
  emptyDescription = 'There are no records to display.',
  className,
}: DataTableProps<T>) {
  const [sortKey, setSortKey] = useState<string | null>(null)
  const [sortDir, setSortDir] = useState<SortDirection>('asc')

  const handleSort = useCallback(
    (key: string) => {
      const nextDir = sortKey === key && sortDir === 'asc' ? 'desc' : 'asc'
      setSortKey(key)
      setSortDir(nextDir)
      onSort?.(key, nextDir)
    },
    [sortKey, sortDir, onSort],
  )

  const total = totalCount ?? data.length
  const totalPages = Math.max(1, Math.ceil(total / pageSize))

  return (
    <div className={cn(styles.wrapper, className)}>
      <div className={styles.scrollArea}>
        <table className={styles.table}>
          <thead>
            <tr>
              {columns.map((col) => (
                <th
                  key={col.key}
                  style={col.width ? { width: col.width } : undefined}
                  className={cn(col.sortable && styles.sortable)}
                  onClick={col.sortable ? () => handleSort(col.key) : undefined}
                >
                  {col.header}
                  {col.sortable && (
                    <span className={styles.sortIcon}>
                      {sortKey === col.key ? (
                        sortDir === 'asc' ? (
                          <ChevronUp size={14} />
                        ) : (
                          <ChevronDown size={14} />
                        )
                      ) : (
                        <ChevronsUpDown size={14} />
                      )}
                    </span>
                  )}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.length === 0 ? (
              <tr>
                <td colSpan={columns.length}>
                  <div className={styles.emptyState}>
                    <div className={styles.emptyIcon}>
                      <Inbox size={40} />
                    </div>
                    <div className={styles.emptyTitle}>{emptyTitle}</div>
                    <div className={styles.emptyDescription}>{emptyDescription}</div>
                  </div>
                </td>
              </tr>
            ) : (
              data.map((row) => (
                <tr key={keyExtractor(row)}>
                  {columns.map((col) => (
                    <td key={col.key}>
                      {col.render ? col.render(row) : String(row[col.key] ?? '')}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className={styles.footer}>
          <span>
            {total} record{total !== 1 ? 's' : ''}
          </span>
          <div className={styles.pagination}>
            <button
              className={styles.pageButton}
              disabled={page <= 1}
              onClick={() => onPageChange?.(page - 1)}
              aria-label="Previous page"
            >
              <ChevronLeft size={16} />
            </button>
            {Array.from({ length: Math.min(totalPages, 5) }, (_, i) => {
              let pageNum: number
              if (totalPages <= 5) {
                pageNum = i + 1
              } else if (page <= 3) {
                pageNum = i + 1
              } else if (page >= totalPages - 2) {
                pageNum = totalPages - 4 + i
              } else {
                pageNum = page - 2 + i
              }
              return (
                <button
                  key={pageNum}
                  className={cn(styles.pageButton, page === pageNum && styles.pageButtonActive)}
                  onClick={() => onPageChange?.(pageNum)}
                >
                  {pageNum}
                </button>
              )
            })}
            <button
              className={styles.pageButton}
              disabled={page >= totalPages}
              onClick={() => onPageChange?.(page + 1)}
              aria-label="Next page"
            >
              <ChevronRight size={16} />
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
