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
  id: string;
  question: string;
  options: string[];
  createdAt: string;
  createdBy: string;
}

export interface PollResults {
  pollId: string;
  results: {
    option: string;
    votes: number;
    percentage: number;
  }[];
}

export interface User {
  id: string;
  username: string;
  email: string;
  createdAt: string;
  createdPollsCount: number;
}

export const pollApi = {
  getPolls: () => api.get<Poll[]>('/polls'),
  getPoll: (id: string) => api.get<Poll>(`/polls/${id}`),
  createPoll: (data: { question: string; options: string[] }) => 
    api.post<Poll>('/polls', data),
  vote: (pollId: string, option: string) => 
    api.post(`/polls/${pollId}/vote`, { option }),
  getResults: (pollId: string) => 
    api.get<PollResults>(`/polls/${pollId}/results`),
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

export default api; 