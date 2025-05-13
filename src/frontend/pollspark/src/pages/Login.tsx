import { Container, Title, TextInput, PasswordInput, Button, Text, Paper, Stack, rem } from '@mantine/core';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface LoginFormData {
  email: string;
  password: string;
}

export function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const { register, handleSubmit, formState: { errors } } = useForm<LoginFormData>();

  const onSubmit = async (data: LoginFormData) => {
    try {
      await login(data.email, data.password);
      navigate('/');
    } catch (error) {
      console.error('Login failed:', error);
    }
  };

  return (
    <Container size="xs" py={rem(40)}>
      <Paper radius="md" p="xl" withBorder shadow="sm">
        <Title order={2} ta="center" mb="xl" c="blue" style={{ letterSpacing: '-0.5px' }}>
          Welcome Back
        </Title>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Stack gap="md">
            <TextInput
              label="Email"
              placeholder="your@email.com"
              {...register('email', {
                required: 'Email is required',
                pattern: {
                  value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                  message: 'Invalid email address',
                },
              })}
              error={errors.email?.message}
              size="md"
              radius="md"
            />
            <PasswordInput
              label="Password"
              placeholder="Your password"
              {...register('password', {
                required: 'Password is required',
              })}
              error={errors.password?.message}
              size="md"
              radius="md"
            />
            <Button type="submit" fullWidth size="md" mt="xl" radius="md">
              Login
            </Button>
            <Text ta="center" size="sm" mt="md" c="dimmed">
              Don't have an account?{' '}
              <Text component={Link} to="/register" size="sm" c="blue" fw={500} style={{ textDecoration: 'none' }}>
                Register
              </Text>
            </Text>
          </Stack>
        </form>
      </Paper>
    </Container>
  );
} 