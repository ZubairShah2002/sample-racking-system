import { useAuthStore } from '@/stores/auth-store'

// Stable empty reference so the Zustand selector never returns a fresh array
// (which would trigger an infinite re-render loop).
const EMPTY_ROLES: string[] = []

const ADMIN_ROLES = ['Administrator', 'QC Manager']
const QC_ROLES = ['Administrator', 'QC Manager', 'QC Technician']

/**
 * Derives the current user's capabilities in the sample tracking module from
 * their roles, mirroring the API's role groups so the UI only offers actions
 * the server will accept.
 */
export function useSampleTrackingAccess() {
  const roles = useAuthStore((s) => s.user?.roles) ?? EMPTY_ROLES
  const isAdmin = roles.some((r) => ADMIN_ROLES.includes(r))
  const canWrite = roles.some((r) => QC_ROLES.includes(r))
  return { isAdmin, canWrite, isViewerOnly: !canWrite }
}
