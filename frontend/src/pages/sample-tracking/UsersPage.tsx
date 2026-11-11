import { useMemo, useState } from 'react'
import { KeyRound, Pencil, Plus, Power, Search } from 'lucide-react'
import { PageHeader } from '@/features/sampleTracking/components/common'
import { Modal } from '@/features/sampleTracking/components/Modal'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Badge } from '@/components/ui/Badge'
import { DataTable } from '@/components/ui/DataTable'
import { toast } from '@/components/feedback/Toast'
import {
  useCreateUser,
  useResetUserPassword,
  useRoles,
  useSetUserActive,
  useUpdateUser,
  useUsers,
  type ManagedUser,
} from '@/features/auth/api'
import shared from '@/features/sampleTracking/components/shared.module.css'

type Draft = {
  username: string
  fullName: string
  email: string
  password: string
  roles: string[]
}

const EMPTY_DRAFT: Draft = { username: '', fullName: '', email: '', password: '', roles: [] }

export default function UsersPage() {
  const [search, setSearch] = useState('')
  const { data: users = [], isLoading } = useUsers(search || undefined, true)
  const { data: roles = [] } = useRoles()
  const createUser = useCreateUser()
  const updateUser = useUpdateUser()
  const setActive = useSetUserActive()
  const resetPassword = useResetUserPassword()

  const [createOpen, setCreateOpen] = useState(false)
  const [editUser, setEditUser] = useState<ManagedUser | null>(null)
  const [resetUser, setResetUser] = useState<ManagedUser | null>(null)
  const [draft, setDraft] = useState<Draft>(EMPTY_DRAFT)
  const [newPassword, setNewPassword] = useState('')

  const roleNames = useMemo(() => roles.map((r) => r.name), [roles])

  function toggleRole(role: string) {
    setDraft((d) => ({
      ...d,
      roles: d.roles.includes(role) ? d.roles.filter((r) => r !== role) : [...d.roles, role],
    }))
  }

  async function submitCreate() {
    try {
      await createUser.mutateAsync(draft)
      okToast(`User "${draft.username}" created`)
      setCreateOpen(false)
      setDraft(EMPTY_DRAFT)
    } catch (e) {
      failToast(errMessage(e, 'Could not create user'))
    }
  }

  async function submitEdit() {
    if (!editUser) return
    try {
      await updateUser.mutateAsync({
        id: editUser.id,
        fullName: draft.fullName,
        email: draft.email,
        roles: draft.roles,
      })
      okToast('User updated')
      setEditUser(null)
    } catch (e) {
      failToast(errMessage(e, 'Could not update user'))
    }
  }

  async function submitReset() {
    if (!resetUser) return
    try {
      await resetPassword.mutateAsync({ id: resetUser.id, newPassword })
      okToast(`Password reset for ${resetUser.username}`)
      setResetUser(null)
      setNewPassword('')
    } catch (e) {
      failToast(errMessage(e, 'Could not reset password'))
    }
  }

  async function toggleActive(u: ManagedUser) {
    try {
      await setActive.mutateAsync({ id: u.id, active: !u.isActive })
      okToast(`${u.username} ${u.isActive ? 'deactivated' : 'activated'}`)
    } catch (e) {
      failToast(errMessage(e, 'Could not change status'))
    }
  }

  return (
    <div className={shared.page}>
      <PageHeader
        title="User Management"
        subtitle="Create and manage accounts, roles, and access for the racking system."
        actions={
          <Button
            icon={<Plus size={16} />}
            onClick={() => {
              setDraft(EMPTY_DRAFT)
              setCreateOpen(true)
            }}
          >
            Add user
          </Button>
        }
      />

      <div className={shared.searchWrapper}>
        <Search size={16} className={shared.searchIcon} />
        <input
          className={shared.searchInput}
          placeholder="Search by username, name, or email…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      <DataTable<ManagedUser>
        data={users}
        keyExtractor={(u) => u.id}
        emptyTitle={isLoading ? 'Loading…' : 'No users'}
        emptyDescription="Create the first user to get started."
        columns={[
          { key: 'username', header: 'Username', render: (u) => <strong>{u.username}</strong> },
          { key: 'fullName', header: 'Full name', render: (u) => u.fullName },
          { key: 'email', header: 'Email', render: (u) => u.email || '—' },
          {
            key: 'roles',
            header: 'Roles',
            render: (u) => (
              <span style={{ display: 'flex', gap: 4, flexWrap: 'wrap' }}>
                {u.roles.map((r) => (
                  <Badge key={r} variant="info">
                    {r}
                  </Badge>
                ))}
              </span>
            ),
          },
          {
            key: 'isActive',
            header: 'Status',
            render: (u) => <Badge variant={u.isActive ? 'success' : 'neutral'}>{u.isActive ? 'Active' : 'Inactive'}</Badge>,
          },
          {
            key: 'lastLoginAtUtc',
            header: 'Last login',
            render: (u) => (u.lastLoginAtUtc ? new Date(u.lastLoginAtUtc).toLocaleString() : 'Never'),
          },
          {
            key: 'actions',
            header: '',
            render: (u) => (
              <span style={{ display: 'flex', gap: 6, justifyContent: 'flex-end' }}>
                <Button
                  variant="ghost"
                  size="sm"
                  icon={<Pencil size={14} />}
                  onClick={() => {
                    setEditUser(u)
                    setDraft({ username: u.username, fullName: u.fullName, email: u.email, password: '', roles: u.roles })
                  }}
                >
                  Edit
                </Button>
                <Button variant="ghost" size="sm" icon={<KeyRound size={14} />} onClick={() => setResetUser(u)}>
                  Reset
                </Button>
                <Button
                  variant={u.isActive ? 'ghost' : 'secondary'}
                  size="sm"
                  icon={<Power size={14} />}
                  onClick={() => toggleActive(u)}
                >
                  {u.isActive ? 'Disable' : 'Enable'}
                </Button>
              </span>
            ),
          },
        ]}
      />

      {/* Create */}
      <Modal open={createOpen} title="Add user" onClose={() => setCreateOpen(false)} width={520}>
        <div className={shared.form}>
          <div className={shared.formGrid}>
            <Input label="Username" value={draft.username} onChange={(e) => setDraft({ ...draft, username: e.target.value })} required />
            <Input label="Full name" value={draft.fullName} onChange={(e) => setDraft({ ...draft, fullName: e.target.value })} />
            <Input label="Email" type="email" value={draft.email} onChange={(e) => setDraft({ ...draft, email: e.target.value })} />
            <Input
              label="Temporary password"
              type="password"
              value={draft.password}
              onChange={(e) => setDraft({ ...draft, password: e.target.value })}
              hint="Minimum 8 characters."
              required
            />
          </div>
          <RolePicker roleNames={roleNames} selected={draft.roles} onToggle={toggleRole} />
          <div className={shared.formActions}>
            <Button variant="secondary" onClick={() => setCreateOpen(false)}>
              Cancel
            </Button>
            <Button onClick={submitCreate} disabled={createUser.isPending || !draft.username || draft.password.length < 8 || draft.roles.length === 0}>
              Create user
            </Button>
          </div>
        </div>
      </Modal>

      {/* Edit */}
      <Modal open={!!editUser} title={`Edit ${editUser?.username ?? ''}`} onClose={() => setEditUser(null)} width={520}>
        <div className={shared.form}>
          <div className={shared.formGrid}>
            <Input label="Full name" value={draft.fullName} onChange={(e) => setDraft({ ...draft, fullName: e.target.value })} />
            <Input label="Email" type="email" value={draft.email} onChange={(e) => setDraft({ ...draft, email: e.target.value })} />
          </div>
          <RolePicker roleNames={roleNames} selected={draft.roles} onToggle={toggleRole} />
          <div className={shared.formActions}>
            <Button variant="secondary" onClick={() => setEditUser(null)}>
              Cancel
            </Button>
            <Button onClick={submitEdit} disabled={updateUser.isPending || draft.roles.length === 0}>
              Save changes
            </Button>
          </div>
        </div>
      </Modal>

      {/* Reset password */}
      <Modal open={!!resetUser} title={`Reset password — ${resetUser?.username ?? ''}`} onClose={() => setResetUser(null)} width={420}>
        <div className={shared.form}>
          <Input
            label="New password"
            type="password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            hint="Minimum 8 characters."
            required
          />
          <div className={shared.formActions}>
            <Button variant="secondary" onClick={() => setResetUser(null)}>
              Cancel
            </Button>
            <Button onClick={submitReset} disabled={resetPassword.isPending || newPassword.length < 8}>
              Reset password
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  )
}

function RolePicker({
  roleNames,
  selected,
  onToggle,
}: {
  roleNames: string[]
  selected: string[]
  onToggle: (role: string) => void
}) {
  return (
    <div className={shared.field}>
      <span className={shared.label}>Roles</span>
      <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
        {roleNames.map((role) => {
          const active = selected.includes(role)
          return (
            <button
              key={role}
              type="button"
              onClick={() => onToggle(role)}
              style={{
                padding: '6px 12px',
                borderRadius: 999,
                border: `1px solid ${active ? 'var(--color-primary-600)' : 'var(--color-border)'}`,
                background: active ? 'var(--color-primary-600)' : 'transparent',
                color: active ? '#fff' : 'var(--color-neutral-700)',
                cursor: 'pointer',
                fontSize: '0.82rem',
              }}
            >
              {role}
            </button>
          )
        })}
      </div>
    </div>
  )
}

const okToast = (title: string) => toast({ type: 'success', title })
const failToast = (title: string) => toast({ type: 'error', title })

function errMessage(e: unknown, fallback: string): string {
  return (e as { response?: { data?: { error?: string } } })?.response?.data?.error ?? fallback
}
