import { useState } from 'react';
import { Container, Title, Text, Card, Group, Stack, Button, Pagination, Badge, Skeleton, SegmentedControl } from '@mantine/core';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { pollApi, type Poll, type PollResults } from '../services/api';
import { formatDistanceToNow } from 'date-fns';

type PollFilter = 'all' | 'active' | 'expired' | 'voted';

export function Polls() {
  const [page, setPage] = useState(1);
  const [filter, setFilter] = useState<PollFilter>('all');
  const pageSize = 10;

  const { data: response, isLoading, error } = useQuery({
    queryKey: ['polls', page, filter],
    queryFn: () => {
      if (filter === 'voted') {
        return pollApi.getVotedPolls(page, pageSize);
      }
      return pollApi.getPolls(page, pageSize);
    },
  });

  const { data: resultsResponses } = useQuery({
    queryKey: ['poll-results', response?.data?.items?.$values?.map(p => p.id)],
    queryFn: async () => {
      const polls = response?.data?.items?.$values || [];
      const results = await Promise.all(
        polls.map(poll => pollApi.getResults(poll.id).catch(() => null))
      );
      return results.reduce((acc, result, index) => {
        if (result?.data) {
          acc[polls[index].id] = result.data;
        }
        return acc;
      }, {} as Record<string, PollResults>);
    },
    enabled: !!response?.data?.items?.$values?.length,
  });

  const data = response?.data;
  const items = data?.items?.$values || [];
  
  console.log('Full response:', response);
  console.log('Extracted data:', data);
  console.log('Items:', items);
  console.log('Items type:', items ? typeof items : 'undefined');
  console.log('Is array?', Array.isArray(items));

  const filteredItems = items.filter((poll) => {
    const isExpired = poll.expiresAt && new Date(poll.expiresAt) < new Date();
    switch (filter) {
      case 'active':
        return !isExpired;
      case 'expired':
        return isExpired;
      default:
        return true;
    }
  });

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
      <Group justify="space-between" align="center" mb="xl">
        <Title order={1}>Browse Polls</Title>
        <SegmentedControl
          value={filter}
          onChange={(value) => {
            setFilter(value as PollFilter);
            setPage(1); // Reset to first page when changing filter
          }}
          data={[
            { label: 'All', value: 'all' },
            { label: 'Active', value: 'active' },
            { label: 'Expired', value: 'expired' },
            { label: 'My Votes', value: 'voted' },
          ]}
        />
      </Group>

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
          filteredItems.map((poll: Poll) => {
            const isExpired = poll.expiresAt && new Date(poll.expiresAt) < new Date();
            const results = resultsResponses?.[poll.id];
            return (
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
                    <Group>
                      {isExpired && (
                        <Badge color="red">Expired</Badge>
                      )}
                      <Badge color={poll.isPublic ? 'blue' : 'gray'}>
                        {poll.isPublic ? 'Public' : 'Private'}
                      </Badge>
                    </Group>
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

                  {results && isExpired && (
                    <Text size="sm" c="dimmed" ta="center">
                      Total votes: {results.totalVotes}
                    </Text>
                  )}

                  <Group gap="xs" mt="xs">
                    {poll.options.$values.map((option) => {
                      const result = results?.results.$values.find(r => r.optionId.toLowerCase() === option.id.toLowerCase());
                      return (
                        <Badge 
                          key={option.id} 
                          variant="light"
                          style={{ minWidth: 80, padding: '0 8px', display: 'inline-flex', alignItems: 'center', justifyContent: 'center', whiteSpace: 'nowrap' }}
                        >
                          <Group gap={4} align="center" style={{ flexWrap: 'nowrap' }}>
                            <span style={{ overflow: 'hidden', textOverflow: 'ellipsis', maxWidth: 90 }}>{option.text}</span>
                            {results && isExpired && (
                              <Text size="xs" c="dimmed" span>
                                ({result?.votes || 0})
                              </Text>
                            )}
                          </Group>
                        </Badge>
                      );
                    })}
                  </Group>

                  <Group justify="flex-end" mt="md">
                    <Button component={Link} to={`/polls/${poll.id}`} variant="light">
                      View Poll
                    </Button>
                  </Group>
                </Stack>
              </Card>
            );
          })
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