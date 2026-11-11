import { useState } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import {
  Bell,
  Boxes,
  Box,
  CalendarClock,
  ClipboardCheck,
  FileBarChart,
  FileSpreadsheet,
  Grid3x3,
  LayoutDashboard,
  LogOut,
  Menu,
  Move,
  QrCode,
  ScanLine,
  Settings,
  Trash2,
  Users,
  Warehouse,
} from 'lucide-react'
import { useAuthStore } from '@/stores/auth-store'
import { useSampleTrackingAccess } from '@/features/sampleTracking/useAccess'
import { cn } from '@/lib/cn'
import styles from './AppShell.module.css'

interface NavItem {
  to: string
  label: string
  icon: React.ReactNode
  admin?: boolean
  qc?: boolean
  end?: boolean
}

const NAV: { section: string; items: NavItem[] }[] = [
  {
    section: 'Overview',
    items: [
      { to: '/sample-tracking/dashboard', label: 'Dashboard', icon: <LayoutDashboard size={18} /> },
      { to: '/sample-tracking/rack-3d', label: '3D Rack', icon: <Grid3x3 size={18} /> },
      { to: '/sample-tracking/rack', label: 'Rack Overview', icon: <Warehouse size={18} /> },
    ],
  },
  {
    section: 'Samples',
    items: [
      { to: '/sample-tracking/register', label: 'Register / Receive', icon: <Box size={18} />, qc: true },
      { to: '/sample-tracking/inventory', label: 'Inventory', icon: <Boxes size={18} /> },
      { to: '/sample-tracking/testing', label: 'Testing', icon: <ClipboardCheck size={18} /> },
      { to: '/sample-tracking/aging', label: 'Aging / Overdue', icon: <CalendarClock size={18} /> },
      { to: '/sample-tracking/movement', label: 'Movement History', icon: <Move size={18} /> },
      { to: '/sample-tracking/disposal', label: 'Disposal', icon: <Trash2 size={18} /> },
      { to: '/sample-tracking/scan', label: 'Scan', icon: <ScanLine size={18} /> },
    ],
  },
  {
    section: 'Tools',
    items: [
      { to: '/sample-tracking/labels', label: 'QR Labels', icon: <QrCode size={18} /> },
      { to: '/sample-tracking/reports', label: 'Reports', icon: <FileBarChart size={18} /> },
      { to: '/sample-tracking/import', label: 'Excel Import', icon: <FileSpreadsheet size={18} />, admin: true },
      { to: '/sample-tracking/notifications', label: 'Notifications', icon: <Bell size={18} /> },
    ],
  },
  {
    section: 'Administration',
    items: [
      { to: '/sample-tracking/users', label: 'Users', icon: <Users size={18} />, admin: true },
      { to: '/sample-tracking/settings', label: 'Settings', icon: <Settings size={18} />, admin: true },
    ],
  },
]

export function AppShell() {
  const navigate = useNavigate()
  const user = useAuthStore((s) => s.user)
  const clearAuth = useAuthStore((s) => s.clearAuth)
  const { isAdmin, canWrite } = useSampleTrackingAccess()
  const [open, setOpen] = useState(false)

  function visible(item: NavItem) {
    if (item.admin) return isAdmin
    if (item.qc) return canWrite
    return true
  }

  function logout() {
    clearAuth()
    navigate('/login', { replace: true })
  }

  const initials = (user?.fullName ?? user?.username ?? '?')
    .split(' ')
    .map((p) => p[0])
    .slice(0, 2)
    .join('')
    .toUpperCase()

  return (
    <div className={styles.shell}>
      <aside className={cn(styles.sidebar, open && styles.sidebarOpen)}>
        <div className={styles.brand}>
          <span className={styles.brandLogo}>
            <Warehouse size={20} />
          </span>
          <div className={styles.brandText}>
            <strong>Sample Racking</strong>
            <span>Management System</span>
          </div>
        </div>

        <nav className={styles.nav}>
          {NAV.map((group) => {
            const items = group.items.filter(visible)
            if (items.length === 0) return null
            return (
              <div key={group.section} className={styles.navGroup}>
                <p className={styles.navSection}>{group.section}</p>
                {items.map((item) => (
                  <NavLink
                    key={item.to}
                    to={item.to}
                    onClick={() => setOpen(false)}
                    className={({ isActive }) => cn(styles.navLink, isActive && styles.navLinkActive)}
                  >
                    <span className={styles.navIcon}>{item.icon}</span>
                    {item.label}
                  </NavLink>
                ))}
              </div>
            )
          })}
        </nav>
      </aside>

      {open && <div className={styles.backdrop} onClick={() => setOpen(false)} />}

      <div className={styles.main}>
        <header className={styles.topbar}>
          <button className={styles.menuButton} onClick={() => setOpen((v) => !v)} aria-label="Toggle menu">
            <Menu size={20} />
          </button>
          <div className={styles.spacer} />
          <div className={styles.user}>
            <div className={styles.userInfo}>
              <strong>{user?.fullName ?? user?.username}</strong>
              <span>{user?.roles?.join(', ')}</span>
            </div>
            <span className={styles.avatar}>{initials}</span>
            <button className={styles.logout} onClick={logout} title="Sign out">
              <LogOut size={18} />
            </button>
          </div>
        </header>

        <main className={styles.content}>
          <Outlet />
        </main>
      </div>
    </div>
  )
}
