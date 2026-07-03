const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5071";
// Пока что .env не создавал
export async function apiRequest(path, options = {}) {
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers,
    },
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || `HTTP error: ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}
