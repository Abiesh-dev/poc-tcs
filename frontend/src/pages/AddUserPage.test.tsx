import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ApiError } from '../api/ApiError';
import * as usersApi from '../api/users';
import { renderWithProviders } from '../test/renderWithProviders';
import type { User } from '../types/user';
import { AddUserPage } from './AddUserPage';

const navigate = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
  return { ...actual, useNavigate: () => navigate };
});

const createdUser: User = {
  id: 1,
  name: 'Ada Lovelace',
  age: 36,
  city: 'London',
  state: 'Greater London',
  pincode: '600001',
  createdAtUtc: '2026-01-01T00:00:00Z',
  updatedAtUtc: '2026-01-01T00:00:00Z',
};

const fillForm = async () => {
  await userEvent.type(screen.getByLabelText('Name'), 'Ada Lovelace');
  await userEvent.type(screen.getByLabelText('Age'), '36');
  await userEvent.type(screen.getByLabelText('City'), 'London');
  await userEvent.type(screen.getByLabelText('State'), 'Greater London');
  await userEvent.type(screen.getByLabelText('Pincode'), '600001');
};

const submit = () => userEvent.click(screen.getByRole('button', { name: 'Save user' }));

describe('AddUserPage', () => {
  it('shows inline validation messages and does not call the API', async () => {
    const createUser = vi.spyOn(usersApi, 'createUser');

    renderWithProviders(<AddUserPage />);
    await submit();

    expect(screen.getByText('Name is required.')).toBeInTheDocument();
    expect(screen.getByText('Age is required.')).toBeInTheDocument();
    expect(screen.getByText('City is required.')).toBeInTheDocument();
    expect(screen.getByText('State is required.')).toBeInTheDocument();
    expect(screen.getByText('Pincode is required.')).toBeInTheDocument();
    expect(createUser).not.toHaveBeenCalled();
  });

  it('marks invalid fields for assistive technology', async () => {
    renderWithProviders(<AddUserPage />);
    await submit();

    expect(screen.getByLabelText('Name')).toHaveAttribute('aria-invalid', 'true');
  });

  it('clears a field error once the user edits that field', async () => {
    renderWithProviders(<AddUserPage />);
    await submit();

    expect(screen.getByText('Name is required.')).toBeInTheDocument();

    await userEvent.type(screen.getByLabelText('Name'), 'Ada');

    expect(screen.queryByText('Name is required.')).not.toBeInTheDocument();
  });

  it('rejects an out-of-range age before submitting', async () => {
    const createUser = vi.spyOn(usersApi, 'createUser');

    renderWithProviders(<AddUserPage />);
    await fillForm();
    await userEvent.clear(screen.getByLabelText('Age'));
    await userEvent.type(screen.getByLabelText('Age'), '150');
    await submit();

    expect(screen.getByText('Age must be between 0 and 120.')).toBeInTheDocument();
    expect(createUser).not.toHaveBeenCalled();
  });

  it('submits trimmed values, shows a success toast and redirects to the list', async () => {
    const createUser = vi.spyOn(usersApi, 'createUser').mockResolvedValue(createdUser);

    renderWithProviders(<AddUserPage />);
    await fillForm();
    await submit();

    await waitFor(() =>
      expect(createUser).toHaveBeenCalledWith({
        name: 'Ada Lovelace',
        age: 36,
        city: 'London',
        state: 'Greater London',
        pincode: '600001',
      }),
    );

    expect(await screen.findByText('User added successfully.')).toBeInTheDocument();
    expect(navigate).toHaveBeenCalledWith('/');
  });

  it('maps server-side validation errors onto the matching fields', async () => {
    vi.spyOn(usersApi, 'createUser').mockRejectedValue(
      new ApiError('Validation failed', 400, { Pincode: ['Pincode must be between 4 and 10 characters.'] }),
    );

    renderWithProviders(<AddUserPage />);
    await fillForm();
    await submit();

    expect(
      await screen.findByText('Pincode must be between 4 and 10 characters.'),
    ).toBeInTheDocument();
    expect(navigate).not.toHaveBeenCalledWith('/');
  });

  it('shows a conflict message when the user already exists', async () => {
    vi.spyOn(usersApi, 'createUser').mockRejectedValue(
      new ApiError("A user named 'Ada Lovelace' already exists for pincode '600001'.", 409),
    );

    renderWithProviders(<AddUserPage />);
    await fillForm();
    await submit();

    expect(await screen.findByRole('alert')).toHaveTextContent('already exists for pincode');
  });

  it('reports a network failure without losing the entered values', async () => {
    vi.spyOn(usersApi, 'createUser').mockRejectedValue(
      new ApiError('Unable to reach the server. Check that the API is running.', 0),
    );

    renderWithProviders(<AddUserPage />);
    await fillForm();
    await submit();

    expect(await screen.findByRole('alert')).toHaveTextContent('Unable to reach the server.');
    expect(screen.getByLabelText('Name')).toHaveValue('Ada Lovelace');
  });

  it('re-enables the submit button after a failed save', async () => {
    vi.spyOn(usersApi, 'createUser').mockRejectedValue(new ApiError('Boom', 500));

    renderWithProviders(<AddUserPage />);
    await fillForm();
    await submit();

    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Save user' })).toBeEnabled(),
    );
  });
});
