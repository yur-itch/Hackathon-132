const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5071";
// Пока что .env не создавал
export async function apiRequest(path, options = {}) {
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    credentials: "include", // для того чтобы токен куки отправился в запросе
    headers: {
      "Content-Type": "application/json",
      ...options.headers,
    },
  });

  if (!response.ok) {
    const raw = await response.text();
    let message = raw;

    // [ApiController] на невалидной модели отдаёт ValidationProblemDetails —
    // JSON с errors по полям, а не читаемый текст. Разбираем, если получится.
    try {
      const parsed = JSON.parse(raw);
      if (parsed.errors) {
        message = Object.values(parsed.errors).flat().join(" ");
      } else if (parsed.title) {
        message = parsed.title;
      }
    } catch {
      // не JSON — оставляем как есть
    }

    throw new Error(message || `HTTP error: ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}
