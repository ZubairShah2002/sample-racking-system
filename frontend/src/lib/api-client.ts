import axios from 'axios'
import type { AxiosError, InternalAxiosRequestConfig } from 'axios'

const TOKEN_KEY = 'sr_auth_token'

const apiClient = axios.create({
  baseURL: '/api/v1',
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  config.headers['X-Correlation-Id'] = crypto.randomUUID()
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401 && !window.location.pathname.startsWith('/login')) {
      try {
        localStorage.removeItem(TOKEN_KEY)
        localStorage.removeItem('sr_auth_user')
      } catch {
        /* ignore */
      }
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)

export { apiClient }
