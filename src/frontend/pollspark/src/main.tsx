import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { MantineProvider, createTheme } from '@mantine/core'
import '@mantine/core/styles.css'
import '@mantine/dates/styles.css'
import './index.css'
import App from './App.tsx'

const theme = createTheme({
  primaryColor: 'blue',
  defaultRadius: 'md',
  fontFamily: 'var(--font-sans)',
  components: {
    DateTimePicker: {
      styles: {
        input: {
          height: '42px',
        },
        dropdown: {
          border: '1px solid var(--neutral-200)',
          borderRadius: 'var(--radius-md)',
        },
        timeInput: {
          border: '1px solid var(--neutral-200)',
          borderRadius: 'var(--radius-md)',
        },
      },
    },
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MantineProvider theme={theme}>
      <App />
    </MantineProvider>
  </StrictMode>,
)
