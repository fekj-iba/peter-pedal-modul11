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
  // TODO: POST { username, password } as JSON to /api/auth/login. On 429 and 401 show an error with showError. Otherwise save the token with saveToken. A customer (the token has the claim customer_id, see readClaims) goes to min-sag.html, an employee to sager.html.
}

form.addEventListener('submit', (event) => {
  event.preventDefault();
  login(usernameField.value, passwordField.value).catch((error) => showError(error.message));
});
