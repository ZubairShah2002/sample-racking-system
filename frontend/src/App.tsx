import { Navigate, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from '@/components/ProtectedRoute'
import { AppShell } from '@/components/layout/AppShell'
import LoginPage from '@/pages/LoginPage'

import { SampleDashboardPage } from '@/pages/sample-tracking/SampleDashboardPage'
import { Rack3DPage } from '@/pages/sample-tracking/Rack3DPage'
import { RackOverviewPage } from '@/pages/sample-tracking/RackOverviewPage'
import { SampleRegisterPage } from '@/pages/sample-tracking/SampleRegisterPage'
import { SampleInventoryPage } from '@/pages/sample-tracking/SampleInventoryPage'
import { SampleDetailPage } from '@/pages/sample-tracking/SampleDetailPage'
import { SampleEditPage } from '@/pages/sample-tracking/SampleEditPage'
import { SampleAgingPage } from '@/pages/sample-tracking/SampleAgingPage'
import { SampleTestingPage } from '@/pages/sample-tracking/SampleTestingPage'
import { SampleMovementPage } from '@/pages/sample-tracking/SampleMovementPage'
import { DisposalPage } from '@/pages/sample-tracking/DisposalPage'
import { ScanPage } from '@/pages/sample-tracking/ScanPage'
import { RackLabelsPage } from '@/pages/sample-tracking/RackLabelsPage'
import { ReportsPage } from '@/pages/sample-tracking/ReportsPage'
import { BulkImport } from '@/pages/sample-tracking/BulkImport'
import { NotificationsPage } from '@/pages/sample-tracking/NotificationsPage'
import { SampleSettingsPage } from '@/pages/sample-tracking/SampleSettingsPage'
import UsersPage from '@/pages/sample-tracking/UsersPage'

const ADMIN = ['Administrator', 'QC Manager']

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route
        path="/sample-tracking"
        element={
          <ProtectedRoute>
            <AppShell />
          </ProtectedRoute>
        }
      >
        <Route index element={<Navigate to="dashboard" replace />} />
        <Route path="dashboard" element={<SampleDashboardPage />} />
        <Route path="rack-3d" element={<Rack3DPage />} />
        <Route path="rack" element={<RackOverviewPage />} />
        <Route path="register" element={<SampleRegisterPage />} />
        <Route path="inventory" element={<SampleInventoryPage />} />
        <Route path="samples/:id" element={<SampleDetailPage />} />
        <Route path="samples/:id/edit" element={<SampleEditPage />} />
        <Route path="aging" element={<SampleAgingPage />} />
        <Route path="testing" element={<SampleTestingPage />} />
        <Route path="movement" element={<SampleMovementPage />} />
        <Route path="disposal" element={<DisposalPage />} />
        <Route path="scan" element={<ScanPage />} />
        <Route path="scan/:code" element={<ScanPage />} />
        <Route path="labels" element={<RackLabelsPage />} />
        <Route path="reports" element={<ReportsPage />} />
        <Route path="notifications" element={<NotificationsPage />} />
        <Route
          path="import"
          element={
            <ProtectedRoute anyRole={ADMIN}>
              <BulkImport />
            </ProtectedRoute>
          }
        />
        <Route
          path="settings"
          element={
            <ProtectedRoute anyRole={ADMIN}>
              <SampleSettingsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="users"
          element={
            <ProtectedRoute anyRole={ADMIN}>
              <UsersPage />
            </ProtectedRoute>
          }
        />
      </Route>

      <Route path="/" element={<Navigate to="/sample-tracking/dashboard" replace />} />
      <Route path="*" element={<Navigate to="/sample-tracking/dashboard" replace />} />
    </Routes>
  )
}
