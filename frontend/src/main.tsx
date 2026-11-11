import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter } from 'react-router-dom'
import { queryClient } from '@/lib/query-client'
import { ToastContainer } from '@/components/feedback/Toast'
import App from './App'

import '@/styles/reset.css'
import '@/styles/tokens.css'
import '@/styles/utilities.css'
import '@/styles/global.css'

// Default presentation: light theme, information-dense "office" layout.
document.documentElement.dataset.theme = 'light'
document.documentElement.dataset.density = 'office'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <App />
        <ToastContainer />
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>,
)
