import type { User, UserInput } from '../types/user';
import { request } from './client';

export const getUsers = (signal?: AbortSignal): Promise<User[]> =>
  request<User[]>('/api/users', { signal });

export const getUser = (id: number, signal?: AbortSignal): Promise<User> =>
  request<User>(`/api/users/${id}`, { signal });

export const createUser = (input: UserInput): Promise<User> =>
  request<User>('/api/users', { method: 'POST', body: JSON.stringify(input) });

export const updateUser = (id: number, input: UserInput): Promise<User> =>
  request<User>(`/api/users/${id}`, { method: 'PUT', body: JSON.stringify(input) });

export const deleteUser = (id: number): Promise<void> =>
  request<void>(`/api/users/${id}`, { method: 'DELETE' });
