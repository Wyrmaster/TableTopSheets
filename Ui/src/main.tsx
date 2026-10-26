import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { ApiProvider } from '@/api'
import { SettingsProvider } from '@/app/settings/SettingsProvider'
import { InspectorProvider } from '@/components/inspector/InspectorProvider'
import { Provider } from '@/components/ui/provider'
import App from './App.tsx'
import './styles/global.scss'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Provider>
      <SettingsProvider>
        <ApiProvider>
          <InspectorProvider>
            <App />
          </InspectorProvider>
        </ApiProvider>
      </SettingsProvider>
    </Provider>
  </StrictMode>,
)
