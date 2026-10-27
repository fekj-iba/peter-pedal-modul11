// Keeps the login token for this browser tab. sessionStorage is emptied when the tab is closed.
const TOKEN_KEY = 'token';

export function saveToken(token) {
  // TODO: Task 3: save the token under TOKEN_KEY, so it disappears when the tab is closed.
  // The browser has two kinds of storage: localStorage and sessionStorage. Which one fits?
}

export function getToken() {
  // TODO: Task 3: return the saved token (null if there is none).
  // Read it from the same storage as in saveToken.
  return null;
}

// Reads the claims in the token's middle part (the payload). Anyone can do this: a JWT is signed, not
// encrypted. Fine for choosing a page, but the API never trusts the browser – it checks the signature.
export function readClaims(token) {
  const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
  const bytes = Uint8Array.from(atob(base64), (c) => c.charCodeAt(0));
  return JSON.parse(new TextDecoder().decode(bytes));
}

// Called when the API says "not logged in" (no token, or it has expired).
export function goToLogin() {
  // TODO: Task 3: forget the token, and go to the login page.
  // Remove it from the storage, and set window.location.href to 'login.html'.
}
