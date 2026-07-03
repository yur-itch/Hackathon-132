import { apiRequest } from "./apiClient";

export function register(email, password, displayName) {
  return apiRequest("/api/auth/register", {
    method: "POST",
    body: JSON.stringify({ email, password, displayName }),
  });
}

export function login(email, password) {
  return apiRequest("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
}

export function logout() {
  return apiRequest("/api/auth/logout", {
    method: "POST",
  });
}

export function getMe() {
  return apiRequest("/api/user/me");
}
