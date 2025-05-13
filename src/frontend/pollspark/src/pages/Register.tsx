import { Container, Title, TextInput, PasswordInput, Button, Text, Paper, Stack, rem } from '@mantine/core';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface RegisterFormData {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export function Register() {
  const { register: registerUser } = useAuth();
  const navigate = useNavigate();
  const { register, handleSubmit, formState: { errors }, watch } = useForm<RegisterFormData>();
  const password = watch('password');

  const onSubmit = async (data: RegisterFormData) => {
    try {
      await registerUser(data.username, data.email, data.password);
      navigate('/');
    } catch (error) {
      console.error('Registration failed:', error);
    }
  };

  return (
    <Container size="xs" py={rem(40)}>
      <Paper radius="md" p="xl" withBorder shadow="sm">
        <Title order={2} ta="center" mb="xl" c="blue" style={{ letterSpacing: '-0.5px' }}>
          Create an Account
        </Title>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Stack gap="md">
            <TextInput
              label="Username"
              placeholder="Your username"
              {...register('username', { required: 'Username is required' })}
              error={errors.username?.message}
              size="md"
              radius="md"
            />
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
                minLength: {
                  value: 6,
                  message: 'Password must be at least 6 characters',
                },
              })}
              error={errors.password?.message}
              size="md"
              radius="md"
            />
            <PasswordInput
              label="Confirm Password"
              placeholder="Confirm your password"
              {...register('confirmPassword', {
                required: 'Please confirm your password',
                validate: value => value === password || 'Passwords do not match',
              })}
              error={errors.confirmPassword?.message}
              size="md"
              radius="md"
            />
            <Button type="submit" fullWidth size="md" mt="xl" radius="md">
              Register
            </Button>
            <Text ta="center" size="sm" mt="md" c="dimmed">
              Already have an account?{' '}
              <Text component={Link} to="/login" size="sm" c="blue" fw={500} style={{ textDecoration: 'none' }}>
                Login
              </Text>
            </Text>
          </Stack>
        </form>
      </Paper>
    </Container>
  );
} 