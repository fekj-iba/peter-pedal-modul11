// Keeps the login token for this browser tab. sessionStorage is emptied when the tab is closed.
const TOKEN_KEY = 'token';

export function saveToken(token) {
  // TODO: Save the token in sessionStorage under TOKEN_KEY.
}

export function getToken() {
  // TODO: Return the token from sessionStorage (null if there is none).
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
  // TODO: Remove the token from sessionStorage and go to login.html.
}
