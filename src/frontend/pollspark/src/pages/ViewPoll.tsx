import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Container, Title, Text, Card, Group, Stack, Button, Badge, Progress, Skeleton } from '@mantine/core';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { pollApi } from '../services/api';
import { formatDistanceToNow } from 'date-fns';

export function ViewPoll() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [selectedOption, setSelectedOption] = useState<string | null>(null);

  const { data: pollResponse, isLoading: isLoadingPoll } = useQuery({
    queryKey: ['poll', id],
    queryFn: () => pollApi.getPoll(id!),
    enabled: !!id,
  });

  const { data: userVoteResponse } = useQuery({
    queryKey: ['user-vote', id],
    queryFn: () => pollApi.getUserVote(id!),
    enabled: !!id,
  });

  const { data: resultsResponse } = useQuery({
    queryKey: ['poll-results', id],
    queryFn: () => pollApi.getResults(id!),
    enabled: !!id,
  });

  const poll = pollResponse?.data;
  const results = resultsResponse?.data;

  const voteMutation = useMutation({
    mutationFn: ({ pollId, optionId }: { pollId: string; optionId: string }) =>
      pollApi.vote(pollId, optionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['poll-results', id] });
      queryClient.invalidateQueries({ queryKey: ['user-vote', id] });
      setSelectedOption(null);
    },
  });

  // Set the selected option to the user's current vote when it's loaded
  useEffect(() => {
    if (userVoteResponse?.data !== undefined) {
      setSelectedOption(userVoteResponse.data);
    }
  }, [userVoteResponse?.data]);

  if (isLoadingPoll) {
    return (
      <Container size="lg" py="xl">
        <Card withBorder p="lg" radius="md">
          <Stack gap="md">
            <Skeleton height={32} width="60%" />
            <Skeleton height={20} width="80%" />
            <Stack gap="xs">
              <Skeleton height={40} width="100%" />
              <Skeleton height={40} width="100%" />
              <Skeleton height={40} width="100%" />
            </Stack>
          </Stack>
        </Card>
      </Container>
    );
  }

  if (!poll) {
    return (
      <Container size="lg" py="xl">
        <Text c="red" ta="center">
          Poll not found
        </Text>
      </Container>
    );
  }

  const handleVote = () => {
    if (!selectedOption || !poll) return;
    console.log('Voting with:', { pollId: poll.id, optionId: selectedOption });
    voteMutation.mutate({ pollId: poll.id, optionId: selectedOption });
  };

  const isPollExpired = poll.expiresAt && new Date(poll.expiresAt) < new Date();

  return (
    <Container size="lg" py="xl">
      <Card withBorder p="lg" radius="md">
        <Stack gap="md">
          <Group justify="space-between" align="flex-start">
            <div>
              <Title order={2} mb="xs">
                {poll.title}
              </Title>
              <Text c="dimmed" size="sm" mb="md">
                {poll.description}
              </Text>
            </div>
            <Group>
              {isPollExpired && (
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

          {results && (
            <Text size="sm" c="dimmed" ta="center">
              Total votes: {results.totalVotes}
            </Text>
          )}

          <Stack gap="xs">
            {poll.options.$values.map((option) => {
              const result = results?.results.$values.find(r => r.optionId.toLowerCase() === option.id.toLowerCase());
              return (
                <Card
                  key={option.id}
                  withBorder
                  p="md"
                  radius="md"
                  style={{
                    cursor: isPollExpired ? 'default' : 'pointer',
                    borderColor: selectedOption === option.id ? 'var(--mantine-color-blue-6)' : undefined,
                  }}
                  onClick={() => !isPollExpired && setSelectedOption(option.id)}
                >
                  <Group justify="space-between">
                    <Text>{option.text}</Text>
                    {results && (
                      <Text size="sm" c="dimmed">
                        {result?.votes || 0} votes ({result?.percentage || 0}%)
                      </Text>
                    )}
                  </Group>
                  {results && (
                    <Progress
                      value={result?.percentage || 0}
                      mt="xs"
                      size="sm"
                      color={selectedOption === option.id ? 'blue' : 'gray'}
                    />
                  )}
                </Card>
              );
            })}
          </Stack>

          <Group justify="space-between" mt="md">
            <Button variant="light" onClick={() => navigate('/polls')}>
              Back to Polls
            </Button>
            {!isPollExpired && (
              <Button
                onClick={handleVote}
                disabled={!selectedOption || voteMutation.isPending}
                loading={voteMutation.isPending}
              >
                {userVoteResponse?.data ? 'Change Vote' : 'Vote'}
              </Button>
            )}
          </Group>
        </Stack>
      </Card>
    </Container>
  );
} 