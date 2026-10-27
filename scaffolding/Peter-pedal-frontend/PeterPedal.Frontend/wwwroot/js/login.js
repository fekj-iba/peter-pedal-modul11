import { API_BASE_URL } from './config.js';
import { readClaims, saveToken } from './auth.js';

const form = document.getElementById('login-form');
const usernameField = document.getElementById('username');
const passwordField = document.getElementById('password');
const message = document.getElementById('message');

function showError(text) {
  message.textContent = text;
  message.className = 'message error';
  message.hidden = false;
}

// Uses fetch directly, not rest.js: here a 401 means "wrong password", not "go to the login page".
async function login(username, password) {
  // TODO: Task 4: log in with the API from task 1.
  // 1. POST { username, password } as JSON with fetch to `${API_BASE_URL}/api/auth/login`
  //    (method, a Content-Type header and body: JSON.stringify(...)).
  // 2. If it failed (look at response.status), show an error with showError and stop. Remember the
  //    status code for "too many attempts" from task 5.
  // 3. Otherwise read { token } from await response.json(), and save it with saveToken.
  // 4. readClaims(token).customer_id tells you whether it is a customer: go to min-sag.html,
  //    otherwise to sager.html (window.location.href).
}

form.addEventListener('submit', (event) => {
  event.preventDefault();
  login(usernameField.value, passwordField.value).catch((error) => showError(error.message));
});
