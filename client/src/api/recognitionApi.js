const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5071";

export async function identify(file, organ = "auto", scenario = "") {
  const query = new URLSearchParams();

  if (scenario) {
    query.set("scenario", scenario);
  }

  const formData = new FormData();
  formData.append("image", file);
  formData.append("organ", organ);

  const response = await fetch(
    `${API_URL}/api/recognition/identify${query.toString() ? `?${query}` : ""}`,
    {
      method: "POST",
      credentials: "include",
      body: formData,
    },
  );

  if (!response.ok) {
    throw new Error(`API error: ${response.status} ${response.statusText}`);
  }

  return response.json();
}
