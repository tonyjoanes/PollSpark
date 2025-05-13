import { Container, Title, Text, Button, Group } from '@mantine/core';
import { Link } from 'react-router-dom';

export function Home() {
  return (
    <Container size="lg" py="xl">
      <Title order={1} ta="center" mb="xl">
        Welcome to PollSpark
      </Title>
      <Text size="lg" ta="center" mb="xl">
        Create and participate in engaging polls with real-time results
      </Text>
      <Group justify="center" gap="md">
        <Button component={Link} to="/polls" size="lg">
          Browse Polls
        </Button>
        <Button component={Link} to="/create" size="lg" variant="light">
          Create Poll
        </Button>
      </Group>
    </Container>
  );
} 