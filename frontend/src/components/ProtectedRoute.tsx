import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/stores/auth-store'

const EMPTY: string[] = []

interface ProtectedRouteProps {
  children: ReactNode
  /** If set, the user must hold at least one of these roles. */
  anyRole?: string[]
}

export function ProtectedRoute({ children, anyRole }: ProtectedRouteProps) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const roles = useAuthStore((s) => s.user?.roles) ?? EMPTY
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  if (anyRole && anyRole.length > 0 && !roles.some((r) => anyRole.includes(r))) {
    return <Navigate to="/sample-tracking/dashboard" replace />
  }

  return <>{children}</>
}
