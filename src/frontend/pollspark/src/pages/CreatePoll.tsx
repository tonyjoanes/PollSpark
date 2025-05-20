import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Title, TextInput, Textarea, Button, Group, Stack, Switch, Text, Paper, rem } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useMutation } from '@tanstack/react-query';
import { pollApi } from '../services/api';
import { DateTimePicker } from '@mantine/dates';
import { IconCalendar } from '@tabler/icons-react';

interface CreatePollFormData {
  title: string;
  description: string;
  isPublic: boolean;
  expiresAt: Date | null;
  options: string[];
}

export function CreatePoll() {
  const navigate = useNavigate();
  const [options, setOptions] = useState<string[]>(['', '']); // Start with two empty options

  const form = useForm<CreatePollFormData>({
    initialValues: {
      title: '',
      description: '',
      isPublic: true,
      expiresAt: null,
      options: ['', ''],
    },
    validate: {
      title: (value) => (!value ? 'Title is required' : null),
      description: (value) => (!value ? 'Description is required' : null),
      options: (value) => {
        if (value.length < 2) return 'At least 2 options are required';
        if (value.some((opt) => !opt.trim())) return 'All options must be filled';
        return null;
      },
    },
  });

  const createPollMutation = useMutation({
    mutationFn: (data: CreatePollFormData) => {
      const apiData = {
        title: data.title,
        description: data.description,
        isPublic: data.isPublic,
        options: data.options,
        expiresAt: data.expiresAt ? data.expiresAt.toISOString() : undefined,
      };
      return pollApi.createPoll(apiData);
    },
    onSuccess: () => {
      navigate('/polls');
    },
  });

  const addOption = () => {
    setOptions([...options, '']);
    form.setFieldValue('options', [...options, '']);
  };

  const removeOption = (index: number) => {
    if (options.length <= 2) return;
    const newOptions = options.filter((_, i) => i !== index);
    setOptions(newOptions);
    form.setFieldValue('options', newOptions);
  };

  const handleSubmit = (values: CreatePollFormData) => {
    createPollMutation.mutate(values);
  };

  return (
    <Container size="sm" py={rem(40)}>
      <Paper radius="md" p="xl" withBorder shadow="sm">
        <Title order={2} ta="center" mb="xl" c="blue" style={{ letterSpacing: '-0.5px' }}>
          Create New Poll
        </Title>

        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap="md">
            <TextInput
              label="Title"
              placeholder="Enter poll title"
              required
              {...form.getInputProps('title')}
            />

            <Textarea
              label="Description"
              placeholder="Enter poll description"
              required
              minRows={3}
              {...form.getInputProps('description')}
            />

            <Switch
              label="Make this poll public"
              {...form.getInputProps('isPublic', { type: 'checkbox' })}
            />

            <DateTimePicker
              label="Expiration Date (Optional)"
              placeholder="Pick a date and time"
              clearable
              valueFormat="DD MMM YYYY hh:mm A"
              minDate={new Date()}
              leftSection={<IconCalendar size={16} />}
              size="md"
              {...form.getInputProps('expiresAt')}
            />

            <div>
              <Text size="sm" fw={500} mb="xs">
                Options
              </Text>
              <Stack gap="xs">
                {options.map((_, index) => (
                  <Group key={index} gap="xs">
                    <TextInput
                      placeholder={`Option ${index + 1}`}
                      style={{ flex: 1 }}
                      {...form.getInputProps(`options.${index}`)}
                    />
                    {options.length > 2 && (
                      <Button
                        variant="light"
                        color="red"
                        size="sm"
                        onClick={() => removeOption(index)}
                      >
                        Remove
                      </Button>
                    )}
                  </Group>
                ))}
              </Stack>
              <Button
                variant="light"
                size="sm"
                mt="xs"
                onClick={addOption}
              >
                Add Option
              </Button>
            </div>

            <Group justify="space-between" mt="xl">
              <Button variant="light" onClick={() => navigate('/polls')}>
                Cancel
              </Button>
              <Button
                type="submit"
                loading={createPollMutation.isPending}
              >
                Create Poll
              </Button>
            </Group>
          </Stack>
        </form>
      </Paper>
    </Container>
  );
} 