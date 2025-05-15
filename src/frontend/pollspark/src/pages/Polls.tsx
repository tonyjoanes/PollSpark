import { useEffect, useState } from 'react';
import { Container, Title, Text, Card, Group, Stack, Button, Pagination, Badge, Skeleton } from '@mantine/core';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { pollApi, type Poll, type PaginatedResponse } from '../services/api';
import { formatDistanceToNow } from 'date-fns';

export function Polls() {
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const { data: response, isLoading, error } = useQuery({
    queryKey: ['polls', page],
    queryFn: () => pollApi.getPolls(page, pageSize),
  });

  const data = response?.data;
  const items = data?.items?.$values || [];
  
  console.log('Full response:', response);
  console.log('Extracted data:', data);
  console.log('Items:', items);
  console.log('Items type:', items ? typeof items : 'undefined');
  console.log('Is array?', Array.isArray(items));

  if (error) {
    return (
      <Container size="lg" py="xl">
        <Text c="red" ta="center">
          Error loading polls. Please try again later.
        </Text>
      </Container>
    );
  }

  return (
    <Container size="lg" py="xl">
      <Title order={1} mb="xl">
        Browse Polls
      </Title>

      <Stack gap="md">
        {isLoading ? (
          // Loading skeletons
          Array.from({ length: 3 }).map((_, index) => (
            <Card key={index} withBorder p="lg" radius="md">
              <Stack gap="xs">
                <Skeleton height={28} width="60%" />
                <Skeleton height={20} width="80%" />
                <Group>
                  <Skeleton height={24} width={100} />
                  <Skeleton height={24} width={100} />
                </Group>
              </Stack>
            </Card>
          ))
        ) : (
          // Poll cards
          items.map((poll: Poll) => (
            <Card key={poll.id} withBorder p="lg" radius="md">
              <Stack gap="xs">
                <Group justify="space-between" align="flex-start">
                  <div>
                    <Title order={3} mb="xs">
                      {poll.title}
                    </Title>
                    <Text c="dimmed" size="sm" mb="md">
                      {poll.description}
                    </Text>
                  </div>
                  <Badge color={poll.isPublic ? 'blue' : 'gray'}>
                    {poll.isPublic ? 'Public' : 'Private'}
                  </Badge>
                </Group>

                <Group gap="xs">
                  <Text size="sm" c="dimmed">
                    Created by {poll.createdByUsername}
                  </Text>
                  <Text size="sm" c="dimmed">
                    • {formatDistanceToNow(new Date(poll.createdAt), { addSuffix: true })}
                  </Text>
                  {poll.expiresAt && (
                    <>
                      <Text size="sm" c="dimmed">•</Text>
                      <Text size="sm" c="dimmed">
                        Expires {formatDistanceToNow(new Date(poll.expiresAt), { addSuffix: true })}
                      </Text>
                    </>
                  )}
                </Group>

                <Group gap="xs" mt="xs">
                  {poll.options.$values.map((option) => (
                    <Badge key={option.id} variant="light">
                      {option.text}
                    </Badge>
                  ))}
                </Group>

                <Group justify="flex-end" mt="md">
                  <Button component={Link} to={`/polls/${poll.id}`} variant="light">
                    View Poll
                  </Button>
                </Group>
              </Stack>
            </Card>
          ))
        )}

        {data && (
          <Group justify="center" mt="xl">
            <Pagination
              total={data.totalPages}
              value={page}
              onChange={setPage}
              withEdges
              size="md"
            />
          </Group>
        )}
      </Stack>
    </Container>
  );
} 