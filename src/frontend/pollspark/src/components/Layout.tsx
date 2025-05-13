import { AppShell, Button, Group, Container, Box } from '@mantine/core';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export function Layout({ children }: { children: React.ReactNode }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <AppShell header={{ height: 70 }} padding={0}>
      <AppShell.Header>
        <Box bg="white" h="100%">
          <Container h="100%">
            <Group h="100%" justify="space-between" gap="lg">
              <Group gap="lg">
                <Button component={Link} to="/" variant="subtle" color="blue">
                  Home
                </Button>
                {user && (
                  <>
                    <Button component={Link} to="/polls" variant="subtle" color="blue">
                      Polls
                    </Button>
                    <Button component={Link} to="/create" variant="subtle" color="blue">
                      Create Poll
                    </Button>
                  </>
                )}
              </Group>
              <Group gap="lg">
                {user ? (
                  <Button variant="light" color="red" onClick={handleLogout}>
                    Logout
                  </Button>
                ) : (
                  <>
                    <Button component={Link} to="/login" variant="light" color="blue">
                      Login
                    </Button>
                    <Button component={Link} to="/register" variant="filled" color="blue">
                      Register
                    </Button>
                  </>
                )}
              </Group>
            </Group>
          </Container>
        </Box>
      </AppShell.Header>
      <AppShell.Main>
        {children}
      </AppShell.Main>
    </AppShell>
  );
} 