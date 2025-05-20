import { useState } from 'react';
import { Container, Title, Text, Card, Group, Stack, Button, Pagination, Badge, Skeleton, SegmentedControl, Select, Paper, ActionIcon, Tooltip, Chip } from '@mantine/core';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { pollApi, categoryApi, type Poll, type PollResults, type Hashtag } from '../services/api';
import { formatDistanceToNow } from 'date-fns';
import { IconClock, IconUser, IconCalendar, IconBrandX, IconBrandLinkedin, IconX } from '@tabler/icons-react';

type PollFilter = 'all' | 'active' | 'expired' | 'voted';

export function Polls() {
  const [page, setPage] = useState(1);
  const [filter, setFilter] = useState<PollFilter>('all');
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
  const [selectedHashtag, setSelectedHashtag] = useState<string | null>(null);
  const pageSize = 10;

  const { data: categories = [] } = useQuery({
    queryKey: ['categories'],
    queryFn: () => categoryApi.getCategories().then(res => res.data.$values),
  });

  const { data: popularHashtags = [] } = useQuery({
    queryKey: ['popular-hashtags'],
    queryFn: () => pollApi.getPopularHashtags().then(res => res.data.$values),
  });

  const { data: response, isLoading, error } = useQuery({
    queryKey: ['polls', page, filter, selectedCategory, selectedHashtag],
    queryFn: async () => {
      let result;
      if (selectedHashtag) {
        result = await pollApi.getPollsByHashtag(selectedHashtag, page, pageSize);
      } else if (filter === 'voted') {
        result = await pollApi.getVotedPolls(page, pageSize);
      } else {
        result = await pollApi.getPolls(page, pageSize);
      }
      return result;
    },
  });

  const { data: resultsResponses } = useQuery({
    queryKey: ['poll-results', response?.data?.items?.$values?.map((p: Poll) => p.id)],
    queryFn: async () => {
      const polls = response?.data?.items?.$values || [];
      const results = await Promise.all(
        polls.map((poll: Poll) => pollApi.getResults(poll.id).catch(() => null))
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

  console.log('Full Response:', response);
  const data = response?.data;
  console.log('Data:', data);
  const items = data?.items?.$values || [];
  console.log('Items:', items);
  const totalPages = data?.totalPages || 1;
  
  console.log('Selected Hashtag:', selectedHashtag);

  const filteredItems = items.filter((poll: Poll) => {
    const isExpired = poll.expiresAt && new Date(poll.expiresAt) < new Date();
    const matchesCategory = !selectedCategory || 
      poll.categories.$values.some((cat: { id: string }) => cat.id === selectedCategory);
    
    // Don't apply additional filters when a hashtag is selected
    if (selectedHashtag) {
      return true;
    }
    
    switch (filter) {
      case 'active':
        return !isExpired && matchesCategory;
      case 'expired':
        return isExpired && matchesCategory;
      default:
        return matchesCategory;
    }
  });

  console.log('Filtered Items:', filteredItems);

  if (error) {
    console.error('Error:', error);
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
      <Paper radius="md" p="xl" withBorder shadow="sm" mb="xl" style={{ background: 'linear-gradient(45deg, #f3f4f6 0%, #ffffff 100%)' }}>
        <Stack gap="md">
          <Group justify="space-between" align="center">
            <Title order={1} style={{ 
              background: 'linear-gradient(45deg, #228be6 0%, #4dabf7 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              letterSpacing: '-0.5px'
            }}>
              Browse Polls
            </Title>
            <Group>
              <Select
                placeholder="Filter by category"
                clearable
                value={selectedCategory}
                onChange={setSelectedCategory}
                data={categories.map(cat => ({ value: cat.id, label: cat.name }))}
                style={{ width: 200 }}
              />
              <SegmentedControl
                value={filter}
                onChange={(value) => {
                  setFilter(value as PollFilter);
                  setPage(1);
                }}
                data={[
                  { label: 'All', value: 'all' },
                  { label: 'Active', value: 'active' },
                  { label: 'Expired', value: 'expired' },
                  { label: 'My Votes', value: 'voted' },
                ]}
              />
            </Group>
          </Group>

          {selectedHashtag && (
            <Group gap="xs" mt="md">
              <Badge 
                size="lg" 
                color="blue"
                variant="filled"
                style={{
                  background: 'linear-gradient(45deg, #228be6 0%, #4dabf7 100%)',
                  padding: '8px 16px'
                }}
              >
                #{selectedHashtag}
              </Badge>
              <Button
                variant="subtle"
                color="gray"
                size="sm"
                onClick={() => {
                  setSelectedHashtag(null);
                  setPage(1);
                }}
                leftSection={<IconX size={16} />}
              >
                Clear hashtag filter
              </Button>
            </Group>
          )}

          {!selectedHashtag && popularHashtags.length > 0 && (
            <Group gap="xs">
              <Text size="sm" fw={500} c="dimmed">Popular hashtags:</Text>
              {popularHashtags.map((hashtag: Hashtag) => (
                <Chip
                  key={hashtag.id}
                  checked={selectedHashtag === hashtag.name}
                  onChange={() => {
                    setSelectedHashtag(selectedHashtag === hashtag.name ? null : hashtag.name);
                    setPage(1);
                  }}
                  variant="light"
                  color="blue"
                  size="sm"
                >
                  #{hashtag.name}
                </Chip>
              ))}
            </Group>
          )}
        </Stack>
      </Paper>

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
        ) : filteredItems.length > 0 ? (
          // Poll cards
          filteredItems.map((poll: Poll) => {
            const isExpired = poll.expiresAt && new Date(poll.expiresAt) < new Date();
            const results = resultsResponses?.[poll.id];
            return (
              <Card 
                key={poll.id} 
                withBorder 
                p="lg" 
                radius="md"
                style={{
                  background: 'linear-gradient(45deg, #ffffff 0%, #f8f9fa 100%)',
                  transition: 'transform 0.2s ease, box-shadow 0.2s ease',
                  '&:hover': {
                    transform: 'translateY(-2px)',
                    boxShadow: '0 4px 12px rgba(0, 0, 0, 0.1)',
                  }
                }}
              >
                <Stack gap="md">
                  <Group justify="space-between" align="flex-start">
                    <div>
                      <Title order={3} mb="xs" style={{ color: '#1c7ed6' }}>
                        {poll.title}
                      </Title>
                      <Text c="dimmed" size="sm" mb="md">
                        {poll.description}
                      </Text>
                    </div>
                    <Group>
                      {isExpired && (
                        <Badge color="red" variant="light" size="lg">Expired</Badge>
                      )}
                      <Badge 
                        color={poll.isPublic ? 'blue' : 'gray'} 
                        variant="light"
                        size="lg"
                      >
                        {poll.isPublic ? 'Public' : 'Private'}
                      </Badge>
                    </Group>
                  </Group>

                  <Group gap="xs" c="dimmed">
                    <Group gap={4}>
                      <IconUser size={16} />
                      <Text size="sm">{poll.createdByUsername}</Text>
                    </Group>
                    <Text size="sm">•</Text>
                    <Group gap={4}>
                      <IconClock size={16} />
                      <Text size="sm">
                        {formatDistanceToNow(new Date(poll.createdAt), { addSuffix: true })}
                      </Text>
                    </Group>
                    {poll.expiresAt && (
                      <>
                        <Text size="sm">•</Text>
                        <Group gap={4}>
                          <IconCalendar size={16} />
                          <Text size="sm">
                            Expires {formatDistanceToNow(new Date(poll.expiresAt), { addSuffix: true })}
                          </Text>
                        </Group>
                      </>
                    )}
                  </Group>

                  {poll.categories.$values.length > 0 && (
                    <Group gap="xs">
                      {poll.categories.$values.map(category => (
                        <Badge 
                          key={category.id} 
                          color="teal" 
                          variant="light"
                          size="lg"
                          style={{ 
                            background: 'linear-gradient(45deg, #12b886 0%, #20c997 100%)',
                            color: 'white'
                          }}
                        >
                          {category.name}
                        </Badge>
                      ))}
                    </Group>
                  )}

                  {poll.hashtags.$values.length > 0 && (
                    <Group gap="xs">
                      {poll.hashtags.$values.map(hashtag => (
                        <Chip
                          key={hashtag.id}
                          variant="light"
                          color="blue"
                          size="sm"
                          onClick={() => {
                            setSelectedHashtag(hashtag.name);
                            setPage(1);
                          }}
                          style={{ cursor: 'pointer' }}
                        >
                          #{hashtag.name}
                        </Chip>
                      ))}
                    </Group>
                  )}

                  {results && isExpired && (
                    <Text size="sm" c="dimmed" ta="center" fw={500}>
                      Total votes: {results.totalVotes}
                    </Text>
                  )}

                  <Group gap="xs" mt="xs">
                    {poll.options.$values.map((option) => {
                      const result = results?.results.$values.find((r: { optionId: string }) => r.optionId.toLowerCase() === option.id.toLowerCase());
                      return (
                        <Button
                          key={option.id}
                          variant="filled"
                          color="blue"
                          size="md"
                          component={Link}
                          to={`/polls/${poll.id}`}
                          style={{
                            minWidth: 140,
                            height: 'auto',
                            padding: '12px 20px',
                            whiteSpace: 'normal',
                            textAlign: 'left',
                            display: 'flex',
                            flexDirection: 'column',
                            alignItems: 'flex-start',
                            gap: 6,
                            background: 'linear-gradient(45deg, #228be6 0%, #339af0 100%)',
                            border: 'none',
                            boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
                            transition: 'all 0.2s ease',
                            '&:hover': {
                              transform: 'translateY(-2px)',
                              boxShadow: '0 4px 8px rgba(0, 0, 0, 0.15)',
                              background: 'linear-gradient(45deg, #1c7ed6 0%, #228be6 100%)',
                            }
                          }}
                        >
                          <Text 
                            size="sm" 
                            fw={500}
                            style={{ 
                              lineHeight: 1.4,
                              color: 'white',
                              letterSpacing: '0.2px'
                            }}
                          >
                            {option.text}
                          </Text>
                          {results && isExpired && (
                            <Text 
                              size="xs" 
                              style={{ 
                                color: 'rgba(255, 255, 255, 0.9)',
                                fontWeight: 500
                              }}
                            >
                              {result?.votes || 0} votes
                            </Text>
                          )}
                        </Button>
                      );
                    })}
                  </Group>

                  <Group justify="flex-end" mt="md">
                    <Group gap="xs">
                      <Tooltip label="Share on X">
                        <ActionIcon
                          variant="light"
                          color="blue"
                          onClick={() => {
                            const text = encodeURIComponent(`Check out this poll: ${poll.title} - ${poll.description}`);
                            const url = encodeURIComponent(window.location.origin + `/polls/${poll.id}`);
                            window.open(`https://twitter.com/intent/tweet?text=${text}&url=${url}`, '_blank');
                          }}
                        >
                          <IconBrandX size={18} />
                        </ActionIcon>
                      </Tooltip>
                      <Tooltip label="Share on LinkedIn">
                        <ActionIcon
                          variant="light"
                          color="blue"
                          onClick={() => {
                            const text = encodeURIComponent(`Check out this poll: ${poll.title} - ${poll.description}`);
                            const url = encodeURIComponent(window.location.origin + `/polls/${poll.id}`);
                            window.open(`https://www.linkedin.com/sharing/share-offsite/?url=${url}`, '_blank');
                          }}
                        >
                          <IconBrandLinkedin size={18} />
                        </ActionIcon>
                      </Tooltip>
                      <Button 
                        component={Link} 
                        to={`/polls/${poll.id}`} 
                        variant="light"
                        color="blue"
                        style={{
                          background: 'linear-gradient(45deg, #228be6 0%, #4dabf7 100%)',
                          color: 'white',
                          '&:hover': {
                            background: 'linear-gradient(45deg, #1c7ed6 0%, #339af0 100%)',
                          }
                        }}
                      >
                        View Poll
                      </Button>
                    </Group>
                  </Group>
                </Stack>
              </Card>
            );
          })
        ) : (
          <Text ta="center" c="dimmed">
            No polls found
          </Text>
        )}

        {data && (
          <Group justify="center" mt="xl">
            <Pagination
              total={totalPages}
              value={page}
              onChange={setPage}
              withEdges
              size="md"
              styles={{
                control: {
                  '&[data-active]': {
                    background: 'linear-gradient(45deg, #228be6 0%, #4dabf7 100%)',
                  }
                }
              }}
            />
          </Group>
        )}
      </Stack>
    </Container>
  );
} 