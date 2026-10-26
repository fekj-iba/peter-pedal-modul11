import { API_BASE_URL } from './config.js';
import { getToken, goToLogin } from './auth.js';

// GraphQL is just an HTTP POST with { query, variables }.
// Errors come back with HTTP 200 in an "errors" array, so we check for them ourselves.
export async function gql(query, variables = {}) {
  const headers = { 'Content-Type': 'application/json', Accept: 'application/json' };
  // TODO: If there is a token (getToken), send it as the header "Authorization: Bearer <token>".
  const response = await fetch(`${API_BASE_URL}/graphql`, {
    method: 'POST',
    headers,
    body: JSON.stringify({ query, variables }),
  });
  const result = await response.json();
  if (result.errors) {
    // TODO: If an error has extensions.code "AUTH_NOT_AUTHENTICATED" (no token, or it has expired), call goToLogin.
    throw new Error(result.errors.map((e) => e.message).join(' '));
  }
  return result.data;
}
