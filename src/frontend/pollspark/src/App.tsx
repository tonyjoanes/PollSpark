import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { MantineProvider, createTheme } from '@mantine/core';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Layout } from './components/Layout';
import { Home } from './pages/Home';
import { Register } from './pages/Register';
import { Login } from './pages/Login';
import { Polls } from './pages/Polls';
import { ViewPoll } from './pages/ViewPoll';
import { CreatePoll } from './pages/CreatePoll';
import { AuthProvider } from './contexts/AuthContext';
import '@mantine/core/styles.css';
import './App.css';

// Create a client
const queryClient = new QueryClient();

// Create a theme
const theme = createTheme({
  primaryColor: 'blue',
  fontFamily: 'system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
  components: {
    Button: {
      defaultProps: {
        radius: 'md',
      },
    },
    TextInput: {
      defaultProps: {
        radius: 'md',
      },
    },
    PasswordInput: {
      defaultProps: {
        radius: 'md',
      },
    },
    Paper: {
      defaultProps: {
        radius: 'md',
        shadow: 'sm',
      },
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <MantineProvider theme={theme} defaultColorScheme="light">
        <AuthProvider>
          <Router>
            <Layout>
              <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/polls" element={<Polls />} />
                <Route path="/polls/:id" element={<ViewPoll />} />
                <Route path="/create" element={<CreatePoll />} />
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />
              </Routes>
            </Layout>
          </Router>
        </AuthProvider>
      </MantineProvider>
    </QueryClientProvider>
  );
}

export default App;
