import { describe, expect, it, vi } from 'vitest';
import { ApiError } from './ApiError';
import { request } from './client';

const jsonResponse = (status: number, body: unknown): Response =>
  new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });

describe('request', () => {
  it('returns the parsed body for a successful response', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(jsonResponse(200, [{ id: 1 }])));

    await expect(request('/api/users')).resolves.toEqual([{ id: 1 }]);
  });

  it('returns undefined for a 204 response', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 204 })));

    await expect(request('/api/users/1', { method: 'DELETE' })).resolves.toBeUndefined();
  });

  it('sends a JSON content type only when there is a body', async () => {
    // A Response body can only be read once, so each call needs a fresh instance.
    const fetchMock = vi.fn().mockImplementation(() => Promise.resolve(jsonResponse(200, [])));
    vi.stubGlobal('fetch', fetchMock);

    await request('/api/users');
    const getHeaders = (fetchMock.mock.calls[0][1] as RequestInit).headers as Headers;
    expect(getHeaders.has('Content-Type')).toBe(false);

    await request('/api/users', { method: 'POST', body: '{}' });
    const postHeaders = (fetchMock.mock.calls[1][1] as RequestInit).headers as Headers;
    expect(postHeaders.get('Content-Type')).toBe('application/json');
  });

  it('surfaces validation errors from a ProblemDetails payload', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        jsonResponse(400, {
          title: 'One or more validation errors occurred.',
          errors: { Name: ['Name is required.'] },
        }),
      ),
    );

    const error = await request('/api/users', { method: 'POST', body: '{}' }).catch((e) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).status).toBe(400);
    expect((error as ApiError).isValidationError).toBe(true);
    expect((error as ApiError).fieldErrors.Name).toEqual(['Name is required.']);
  });

  it('uses the problem detail text as the error message', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        jsonResponse(409, { title: 'Duplicate user', detail: 'Already exists.' }),
      ),
    );

    await expect(request('/api/users')).rejects.toThrowError('Already exists.');
  });

  it('falls back to a friendly message when the body carries no detail', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 404 })));

    await expect(request('/api/users/1')).rejects.toThrowError(
      'The requested resource was not found.',
    );
  });

  it('reports a network failure with status zero', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')));

    const error = await request('/api/users').catch((e) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).status).toBe(0);
  });
});
