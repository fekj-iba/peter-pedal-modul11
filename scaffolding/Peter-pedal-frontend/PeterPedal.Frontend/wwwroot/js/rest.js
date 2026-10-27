import { API_BASE_URL } from './config.js';
import { getToken, goToLogin } from './auth.js';

// Sends one HTTP request to the REST API and returns the JSON body (null for 204 No Content).
async function request(method, path, body) {
  const options = { method, headers: { Accept: 'application/json' } };
  // TODO: Task 4: send the token (if there is one).
  // Get it with getToken, and put it in options.headers as the Authorization header: "Bearer <token>".
  if (body !== undefined) {
    options.headers['Content-Type'] = 'application/json';
    options.body = JSON.stringify(body);
  }
  const response = await fetch(API_BASE_URL + path, options);
  // TODO: Task 4: not logged in (no token, or it has expired) → go to the login page.
  // Which status code does the REST API answer with? Then call goToLogin.
  if (!response.ok) {
    // ASP.NET Core sends errors as ProblemDetails JSON: { title, status, errors? }
    const problem = await response.json().catch(() => ({}));
    const messages = problem.errors ? Object.values(problem.errors).flat() : [problem.title];
    throw new Error(messages.join(' ') || `${response.status} ${response.statusText}`);
  }
  return response.status === 204 ? null : response.json();
}

export const get = (path) => request('GET', path);
export const post = (path, body) => request('POST', path, body);
export const put = (path, body) => request('PUT', path, body);
export const del = (path) => request('DELETE', path);
