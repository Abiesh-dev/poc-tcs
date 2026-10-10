import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import * as usersApi from './api/users';
import { App } from './App';
import { renderWithProviders } from './test/renderWithProviders';

describe('App', () => {
  it('renders the top bar with List and Add links', async () => {
    vi.spyOn(usersApi, 'getUsers').mockResolvedValue([]);

    renderWithProviders(<App />);

    expect(await screen.findByRole('navigation', { name: 'Main' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'List' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Add' })).toBeInTheDocument();
    expect(await screen.findByText('No users yet')).toBeInTheDocument();
  });

  it('shows the list page at the root route', async () => {
    vi.spyOn(usersApi, 'getUsers').mockResolvedValue([]);

    renderWithProviders(<App />);

    expect(await screen.findByRole('heading', { name: 'Users', level: 1 })).toBeInTheDocument();
  });

  it('navigates to the add page when the Add link is clicked', async () => {
    vi.spyOn(usersApi, 'getUsers').mockResolvedValue([]);

    renderWithProviders(<App />);

    await userEvent.click(screen.getByRole('link', { name: 'Add' }));

    expect(await screen.findByRole('heading', { name: 'Add user', level: 1 })).toBeInTheDocument();
  });

  it('redirects an unknown route back to the list', async () => {
    vi.spyOn(usersApi, 'getUsers').mockResolvedValue([]);

    renderWithProviders(<App />, { route: '/does-not-exist' });

    expect(await screen.findByRole('heading', { name: 'Users', level: 1 })).toBeInTheDocument();
  });
});
