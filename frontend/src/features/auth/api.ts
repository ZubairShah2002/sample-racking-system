import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { AuthUser } from '@/stores/auth-store'

export interface LoginResponse {
  token: string
  expiresAtUtc: string
  user: AuthUser & { isActive: boolean; lastLoginAtUtc: string | null }
}

export type ManagedUser = {
  id: string
  username: string
  email: string
  fullName: string
  roles: string[]
  isActive: boolean
  lastLoginAtUtc: string | null
}

export interface Role {
  id: string
  name: string
  description: string
  isSystem: boolean
}

export const authKeys = {
  me: ['auth', 'me'] as const,
  users: (search?: string, includeInactive?: boolean) =>
    ['auth', 'users', search ?? '', includeInactive ?? true] as const,
  roles: ['auth', 'roles'] as const,
}

export async function login(username: string, password: string): Promise<LoginResponse> {
  const res = await apiClient.post<LoginResponse>('/auth/login', { username, password })
  return res.data
}

export function useCurrentUser(enabled: boolean) {
  return useQuery({
    queryKey: authKeys.me,
    enabled,
    queryFn: async () => {
      const res = await apiClient.get<ManagedUser>('/auth/me')
      return res.data
    },
  })
}

export function useChangePassword() {
  return useMutation({
    mutationFn: async (body: { currentPassword: string; newPassword: string }) => {
      await apiClient.post('/auth/change-password', body)
    },
  })
}

// ---- User management (admin) ----

export function useUsers(search?: string, includeInactive = true) {
  return useQuery({
    queryKey: authKeys.users(search, includeInactive),
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      params.set('includeInactive', String(includeInactive))
      const res = await apiClient.get<ManagedUser[]>(`/users?${params}`)
      return res.data
    },
  })
}

export function useRoles() {
  return useQuery({
    queryKey: authKeys.roles,
    queryFn: async () => {
      const res = await apiClient.get<Role[]>('/roles')
      return res.data
    },
  })
}

export function useCreateUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (body: {
      username: string
      email: string
      fullName: string
      password: string
      roles: string[]
    }) => {
      const res = await apiClient.post<string>('/users', body)
      return res.data
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['auth', 'users'] }),
  })
}

export function useUpdateUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({
      id,
      ...body
    }: {
      id: string
      fullName: string
      email: string
      roles: string[]
    }) => {
      await apiClient.put(`/users/${id}`, body)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['auth', 'users'] }),
  })
}

export function useSetUserActive() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, active }: { id: string; active: boolean }) => {
      await apiClient.post(`/users/${id}/${active ? 'activate' : 'deactivate'}`)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['auth', 'users'] }),
  })
}

export function useResetUserPassword() {
  return useMutation({
    mutationFn: async ({ id, newPassword }: { id: string; newPassword: string }) => {
      await apiClient.post(`/users/${id}/reset-password`, { newPassword })
    },
  })
}
