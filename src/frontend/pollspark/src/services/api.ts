import axios from 'axios';

const API_URL = 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to add the auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export interface Poll {
  $id: string;
  id: string;
  title: string;
  description: string;
  createdAt: string;
  expiresAt: string;
  isPublic: boolean;
  createdByUsername: string;
  options: {
    $id: string;
    $values: {
      id: string;
      text: string;
    }[];
  };
  categories: {
    $id: string;
    $values: {
      id: string;
      name: string;
      description: string;
    }[];
  };
}

export interface PollResults {
  $id: string;
  pollId: string;
  results: {
    $id: string;
    $values: {
      $id: string;
      optionId: string;
      optionText: string;
      votes: number;
      percentage: number;
    }[];
  };
  totalVotes: number;
}

export interface User {
  id: string;
  username: string;
  email: string;
  createdAt: string;
  createdPollsCount: number;
}

export interface Category {
  $id: string;
  id: string;
  name: string;
  description: string;
}

export interface CategoryResponse {
  $id: string;
  $values: Category[];
}

export interface PaginatedResponse<T> {
  $id: string;
  items: {
    $id: string;
    $values: T[];
  };
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export const pollApi = {
  getPolls: (page: number = 0, pageSize: number = 10) => 
    api.get<PaginatedResponse<Poll>>('/polls', { params: { page, pageSize } }),
  getPoll: (id: string) => api.get<Poll>(`/polls/${id}`),
  createPoll: (data: { 
    title: string;
    description: string;
    isPublic: boolean;
    expiresAt?: string;
    options: string[];
    categoryIds: string[];
  }) => api.post<Poll>('/polls', data),
  vote: (pollId: string, optionId: string) => {
    console.log('Making vote request:', {
      url: `/polls/${pollId}/vote`,
      data: { 
        pollId,
        optionId
      }
    });
    return api.post(`/polls/${pollId}/vote`, { 
      pollId,
      optionId
    }, {
      headers: {
        'Content-Type': 'application/json'
      }
    });
  },
  getResults: (pollId: string) => 
    api.get<PollResults>(`/polls/${pollId}/results`),
  getUserVote: (pollId: string) =>
    api.get<string | null>(`/polls/${pollId}/my-vote`),
  getVotedPolls: (page: number = 0, pageSize: number = 10) =>
    api.get<PaginatedResponse<Poll>>('/polls/my-votes', { params: { page, pageSize } }),
};

export const authApi = {
  login: (email: string, password: string) =>
    api.post<{ token: string; user: User }>('/auth/login', { email, password }),
  register: (username: string, email: string, password: string) =>
    api.post<{ token: string; user: User }>('/auth/register', { username, email, password }),
  getCurrentUser: () => api.get<User>('/users/profile'),
  logout: () => {
    localStorage.removeItem('token');
  },
};

export const categoryApi = {
  getCategories: () => api.get<CategoryResponse>('/categories'),
};

export default api; 