const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5071";

function getOwnerId() {
  return localStorage.getItem("ownerId") || "local";
}

async function request(path, options = {}) {
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      "X-User-Id": getOwnerId(),
      ...options.headers,
    },
  });

  if (!response.ok) {
    throw new Error(`API error: ${response.status} ${response.statusText}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export const api = {
  plants: {
    list(params = {}) {
      const query = new URLSearchParams();

      if (params.search) {
        query.set("search", params.search);
      }

      if (params.difficulty) {
        query.set("difficulty", params.difficulty);
      }

      const queryString = query.toString();
      return request(`/api/plants${queryString ? `?${queryString}` : ""}`);
    },

    get(id) {
      return request(`/api/plants/${id}`);
    },
  },

  userPlants: {
    listMine() {
      return request("/api/userplants");
    },

    create(data) {
      return request("/api/userplants", {
        method: "POST",
        body: JSON.stringify(data),
      });
    },

    remove(id) {
      return request(`/api/userplants/${id}`, {
        method: "DELETE",
      });
    },
  },

  reminders: {
    listMine(due = false) {
      return request(`/api/reminders?due=${due}`);
    },

    markDone(id) {
      return request(`/api/reminders/${id}/done`, {
        method: "POST",
      });
    },
  },

  favorites: {
    listMine() {
      return request("/api/favorites");
    },

    add(plantId) {
      return request(`/api/favorites/${plantId}`, {
        method: "POST",
      });
    },

    remove(plantId) {
      return request(`/api/favorites/${plantId}`, {
        method: "DELETE",
      });
    },
  },
};
