import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { ApiProvider } from '@/api'
import { Provider } from '@/components/ui/provider'
import App from './App.tsx'
import './styles/global.scss'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Provider>
      <ApiProvider>
        <App />
      </ApiProvider>
    </Provider>
  </StrictMode>,
)
