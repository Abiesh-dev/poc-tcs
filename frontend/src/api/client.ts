import { config } from '../config';
import { ApiError, type FieldErrors } from './ApiError';

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: FieldErrors;
}

const parseProblemDetails = async (response: Response): Promise<ProblemDetails> => {
  try {
    const body = (await response.json()) as ProblemDetails;
    return body ?? {};
  } catch {
    return {};
  }
};

const messageFor = (status: number, problem: ProblemDetails): string => {
  if (problem.detail) {
    return problem.detail;
  }
  if (problem.title) {
    return problem.title;
  }

  switch (status) {
    case 404:
      return 'The requested resource was not found.';
    case 500:
      return 'The server could not complete the request. Please try again.';
    default:
      return `Request failed with status ${status}.`;
  }
};

export const request = async <TResponse>(
  path: string,
  init: RequestInit = {},
): Promise<TResponse> => {
  const headers = new Headers(init.headers);
  headers.set('Accept', 'application/json');

  if (init.body !== undefined) {
    headers.set('Content-Type', 'application/json');
  }

  let response: Response;

  try {
    response = await fetch(`${config.apiBaseUrl}${path}`, { ...init, headers });
  } catch {
    throw new ApiError('Unable to reach the server. Check that the API is running.', 0);
  }

  if (!response.ok) {
    const problem = await parseProblemDetails(response);
    throw new ApiError(messageFor(response.status, problem), response.status, problem.errors ?? {});
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  return (await response.json()) as TResponse;
};
