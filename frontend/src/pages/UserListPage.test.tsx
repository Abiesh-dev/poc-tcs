import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ApiError } from '../api/ApiError';
import * as usersApi from '../api/users';
import { renderWithProviders } from '../test/renderWithProviders';
import type { User } from '../types/user';
import { UserListPage } from './UserListPage';

const sampleUsers: User[] = [
  {
    id: 1,
    name: 'Ada Lovelace',
    age: 36,
    city: 'London',
    state: 'Greater London',
    pincode: '600001',
    createdAtUtc: '2026-01-01T00:00:00Z',
    updatedAtUtc: '2026-01-01T00:00:00Z',
  },
  {
    id: 2,
    name: 'Grace Hopper',
    age: 45,
    city: 'New York',
    state: 'New York',
    pincode: '10001',
    createdAtUtc: '2026-01-02T00:00:00Z',
    updatedAtUtc: '2026-01-02T00:00:00Z',
  },
];

describe('UserListPage', () => {
  it('shows a spinner while the request is in flight', () => {
    vi.spyOn(usersApi, 'getUsers').mockReturnValue(new Promise(() => {}));

    renderWithProviders(<UserListPage />);

    expect(screen.getByRole('status')).toHaveTextContent('Loading users');
  });

  it('loads users on mount and renders every column', async () => {
    const getUsers = vi.spyOn(usersApi, 'getUsers').mockResolvedValue(sampleUsers);

    renderWithProviders(<UserListPage />);

    expect(await screen.findByText('Ada Lovelace')).toBeInTheDocument();
    expect(getUsers).toHaveBeenCalledTimes(1);

    const headers = screen.getAllByRole('columnheader').map((cell) => cell.textContent);
    expect(headers).toEqual(['Name', 'Age', 'City', 'State', 'Pincode']);

    expect(screen.getByText('Grace Hopper')).toBeInTheDocument();
    expect(screen.getByText('600001')).toBeInTheDocument();
    expect(screen.getByText('45')).toBeInTheDocument();
  });

  it('renders an empty state when the directory has no users', async () => {
    vi.spyOn(usersApi, 'getUsers').mockResolvedValue([]);

    renderWithProviders(<UserListPage />);

    expect(await screen.findByText('No users yet')).toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  it('shows the API error message when loading fails', async () => {
    vi.spyOn(usersApi, 'getUsers').mockRejectedValue(new ApiError('Server unavailable.', 500));

    renderWithProviders(<UserListPage />);

    expect(await screen.findByRole('alert')).toHaveTextContent('Server unavailable.');
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  it('filters rows as the user searches', async () => {
    vi.spyOn(usersApi, 'getUsers').mockResolvedValue(sampleUsers);

    renderWithProviders(<UserListPage />);
    await screen.findByText('Ada Lovelace');

    await userEvent.type(screen.getByLabelText('Search users'), 'new york');

    expect(screen.getByText('Grace Hopper')).toBeInTheDocument();
    expect(screen.queryByText('Ada Lovelace')).not.toBeInTheDocument();
  });

  it('tells the user when a search matches nothing', async () => {
    vi.spyOn(usersApi, 'getUsers').mockResolvedValue(sampleUsers);

    renderWithProviders(<UserListPage />);
    await screen.findByText('Ada Lovelace');

    await userEvent.type(screen.getByLabelText('Search users'), 'zzzz');

    expect(screen.getByText('No users match your search.')).toBeInTheDocument();
    expect(screen.queryByText('Grace Hopper')).not.toBeInTheDocument();
  });

  it('sorts by a column when its header is clicked', async () => {
    vi.spyOn(usersApi, 'getUsers').mockResolvedValue(sampleUsers);

    renderWithProviders(<UserListPage />);
    await screen.findByText('Ada Lovelace');

    await userEvent.click(screen.getByRole('columnheader', { name: /Age/ }));

    const ages = screen
      .getAllByRole('row')
      .slice(1)
      .map((row) => row.querySelectorAll('td')[1]?.textContent);

    expect(ages).toEqual(['36', '45']);
  });

  it('reloads the list when the refresh button is pressed', async () => {
    const getUsers = vi.spyOn(usersApi, 'getUsers').mockResolvedValue(sampleUsers);

    renderWithProviders(<UserListPage />);
    await screen.findByText('Ada Lovelace');

    await userEvent.click(screen.getByRole('button', { name: /Refresh/ }));

    await waitFor(() => expect(getUsers).toHaveBeenCalledTimes(2));
  });

  it('reloads the list when the retry button is pressed', async () => {
    const getUsers = vi
      .spyOn(usersApi, 'getUsers')
      .mockRejectedValueOnce(new ApiError('Server unavailable.', 500))
      .mockResolvedValueOnce(sampleUsers);

    renderWithProviders(<UserListPage />);

    await userEvent.click(await screen.findByRole('button', { name: 'Try again' }));

    expect(await screen.findByText('Ada Lovelace')).toBeInTheDocument();
    await waitFor(() => expect(getUsers).toHaveBeenCalledTimes(2));
  });
});
