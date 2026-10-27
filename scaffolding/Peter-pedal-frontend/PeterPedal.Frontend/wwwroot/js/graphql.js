import { API_BASE_URL } from './config.js';
import { getToken, goToLogin } from './auth.js';

// GraphQL is just an HTTP POST with { query, variables }.
// Errors come back with HTTP 200 in an "errors" array, so we check for them ourselves.
export async function gql(query, variables = {}) {
  const headers = { 'Content-Type': 'application/json', Accept: 'application/json' };
  // TODO: Task 3: send the token (if there is one).
  // Get it with getToken, and put it in headers as the Authorization header: "Bearer <token>".
  const response = await fetch(`${API_BASE_URL}/graphql`, {
    method: 'POST',
    headers,
    body: JSON.stringify({ query, variables }),
  });
  const result = await response.json();
  if (result.errors) {
    // TODO: Task 3: not logged in (no token, or it has expired) → go to the login page.
    // GraphQL answers HTTP 200. Look for the error code in result.errors (each error's extensions.code);
    // you saw it in task 2. Then call goToLogin.
    throw new Error(result.errors.map((e) => e.message).join(' '));
  }
  return result.data;
}
