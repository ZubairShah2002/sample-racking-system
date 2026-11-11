import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  Dashboard,
  NotificationList,
  Paged,
  RackLocation,
  RackLocationDetail,
  RackOverview,
  SampleDetail,
  SampleSettings,
  SampleSummary,
} from './types'

export const stKeys = {
  rackLocations: (search?: string, includeInactive?: boolean) =>
    ['st', 'rackLocations', search ?? '', includeInactive ?? true] as const,
  rackLocation: (id: string) => ['st', 'rackLocation', id] as const,
  rackOverview: ['st', 'rackOverview'] as const,
  samples: (params: Record<string, unknown>) => ['st', 'samples', params] as const,
  sample: (id: string) => ['st', 'sample', id] as const,
  aging: (category?: string) => ['st', 'aging', category ?? ''] as const,
  dashboard: ['st', 'dashboard'] as const,
  notifications: (unreadOnly?: boolean) => ['st', 'notifications', unreadOnly ?? false] as const,
  settings: ['st', 'settings'] as const,
}

// ---- Queries ----

export function useRackLocations(search?: string, includeInactive = true) {
  return useQuery({
    queryKey: stKeys.rackLocations(search, includeInactive),
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      params.set('includeInactive', String(includeInactive))
      const res = await apiClient.get<RackLocation[]>(`/rack-locations?${params}`)
      return res.data
    },
  })
}

export function useRackLocationDetail(id: string | undefined) {
  return useQuery({
    queryKey: stKeys.rackLocation(id ?? ''),
    enabled: !!id,
    queryFn: async () => {
      const res = await apiClient.get<RackLocationDetail>(`/rack-locations/${id}`)
      return res.data
    },
  })
}

export function useRackOverview() {
  return useQuery({
    queryKey: stKeys.rackOverview,
    queryFn: async () => {
      const res = await apiClient.get<RackOverview>('/rack-overview')
      return res.data
    },
  })
}

export interface SampleListParams {
  search?: string
  status?: string
  agingCategory?: string
  supplierName?: string
  productCategory?: string
  department?: string
  locationCode?: string
  receivedFrom?: string
  receivedTo?: string
  includeDisposed?: boolean
  sortBy?: string
  sortDir?: string
  page?: number
  pageSize?: number
}

export function useSamples(params: SampleListParams) {
  return useQuery({
    queryKey: stKeys.samples(params as Record<string, unknown>),
    queryFn: async () => {
      const qs = new URLSearchParams()
      Object.entries(params).forEach(([k, v]) => {
        if (v !== undefined && v !== null && v !== '') qs.set(k, String(v))
      })
      const res = await apiClient.get<Paged<SampleSummary>>(`/samples?${qs}`)
      return res.data
    },
  })
}

export function useSample(id: string | undefined) {
  return useQuery({
    queryKey: stKeys.sample(id ?? ''),
    enabled: !!id,
    queryFn: async () => {
      const res = await apiClient.get<SampleDetail>(`/samples/${id}`)
      return res.data
    },
  })
}

export function useAgingReport(category?: string) {
  return useQuery({
    queryKey: stKeys.aging(category),
    queryFn: async () => {
      const qs = category ? `?category=${encodeURIComponent(category)}` : ''
      const res = await apiClient.get<SampleSummary[]>(`/sample-aging${qs}`)
      return res.data
    },
  })
}

export function useDashboard() {
  return useQuery({
    queryKey: stKeys.dashboard,
    queryFn: async () => {
      const res = await apiClient.get<Dashboard>('/sample-dashboard')
      return res.data
    },
  })
}

export function useNotifications(unreadOnly = false, take = 50) {
  return useQuery({
    queryKey: stKeys.notifications(unreadOnly),
    queryFn: async () => {
      const res = await apiClient.get<NotificationList>(
        `/sample-notifications?unreadOnly=${unreadOnly}&take=${take}`,
      )
      return res.data
    },
  })
}

export function useSettings() {
  return useQuery({
    queryKey: stKeys.settings,
    queryFn: async () => {
      const res = await apiClient.get<SampleSettings>('/sample-settings')
      return res.data
    },
  })
}

// ---- Mutation helpers ----

function useInvalidateAll() {
  const qc = useQueryClient()
  return () => {
    qc.invalidateQueries({ queryKey: ['st'] })
  }
}

export function useRegisterSample() {
  const invalidate = useInvalidateAll()
  return useMutation({
    mutationFn: async (body: Record<string, unknown>) => {
      const res = await apiClient.post<SampleDetail>('/samples', body)
      return res.data
    },
    onSuccess: invalidate,
  })
}

export function useUpdateSample(id: string) {
  const invalidate = useInvalidateAll()
  return useMutation({
    mutationFn: async (body: Record<string, unknown>) => {
      const res = await apiClient.put<SampleDetail>(`/samples/${id}`, body)
      return res.data
    },
    onSuccess: invalidate,
  })
}

export function useSampleAction(id: string, path: string) {
  const invalidate = useInvalidateAll()
  return useMutation({
    mutationFn: async (body: Record<string, unknown>) => {
      const res = await apiClient.post<SampleDetail>(`/samples/${id}/${path}`, body)
      return res.data
    },
    onSuccess: invalidate,
  })
}

export function useCreateRackLocation() {
  const invalidate = useInvalidateAll()
  return useMutation({
    mutationFn: async (body: Record<string, unknown>) => {
      const res = await apiClient.post('/rack-locations', body)
      return res.data
    },
    onSuccess: invalidate,
  })
}

export function useUpdateRackLocation(id: string) {
  const invalidate = useInvalidateAll()
  return useMutation({
    mutationFn: async (body: Record<string, unknown>) => {
      await apiClient.put(`/rack-locations/${id}`, body)
    },
    onSuccess: invalidate,
  })
}

export function useSetRackLocationActive(id: string) {
  const invalidate = useInvalidateAll()
  return useMutation({
    mutationFn: async (active: boolean) => {
      await apiClient.post(`/rack-locations/${id}/${active ? 'activate' : 'deactivate'}`)
    },
    onSuccess: invalidate,
  })
}

export function useUpdateSettings() {
  const invalidate = useInvalidateAll()
  return useMutation({
    mutationFn: async (body: Record<string, unknown>) => {
      const res = await apiClient.put<SampleSettings>('/sample-settings', body)
      return res.data
    },
    onSuccess: invalidate,
  })
}

export function useMarkNotification() {
  const invalidate = useInvalidateAll()
  return useMutation({
    mutationFn: async ({ id, read }: { id: string; read: boolean }) => {
      await apiClient.post(`/sample-notifications/${id}/${read ? 'read' : 'unread'}`)
    },
    onSuccess: invalidate,
  })
}

export function useMarkAllNotificationsRead() {
  const invalidate = useInvalidateAll()
  return useMutation({
    mutationFn: async () => {
      await apiClient.post('/sample-notifications/read-all')
    },
    onSuccess: invalidate,
  })
}

export function useRefreshAgingAlerts() {
  const invalidate = useInvalidateAll()
  return useMutation({
    mutationFn: async () => {
      const res = await apiClient.post<number>('/sample-aging/refresh-alerts')
      return res.data
    },
    onSuccess: invalidate,
  })
}

/** Extracts a human-readable error message from an Axios error. */
export function apiErrorMessage(err: unknown): string {
  const anyErr = err as { response?: { data?: { error?: string; detail?: string; title?: string } } }
  return (
    anyErr?.response?.data?.error ??
    anyErr?.response?.data?.detail ??
    anyErr?.response?.data?.title ??
    'An unexpected error occurred. Please try again.'
  )
}
