const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5071";
const FAVORITES_KEY = "favoritePlantIds";

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

function readFavoriteIds() {
  try {
    return JSON.parse(localStorage.getItem(FAVORITES_KEY)) || [];
  } catch {
    return [];
  }
}

function saveFavoriteIds(ids) {
  localStorage.setItem(FAVORITES_KEY, JSON.stringify(ids));
}

export const api = {
  plants: {
    list(params = {}) {
      const query = new URLSearchParams();

      if (params.search) {
        query.set("search", params.search);
      }

      if (params.isPoisonous !== undefined && params.isPoisonous !== "") {
        query.set("isPoisonous", params.isPoisonous);
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
      return request("/api/user-plants");
    },

    create(data) {
      return request("/api/user-plants", {
        method: "POST",
        body: JSON.stringify(data),
      });
    },

    update(id, data) {
      return request(`/api/user-plants/${id}`, {
        method: "PUT",
        body: JSON.stringify(data),
      });
    },

    remove(id) {
      return request(`/api/user-plants/${id}`, {
        method: "DELETE",
      });
    },
  },

  favorites: {
    async listMine() {
      const ids = readFavoriteIds();
      const plants = await api.plants.list();
      return plants.filter((plant) => ids.includes(plant.id));
    },

    add(plantId) {
      const ids = readFavoriteIds();

      if (!ids.includes(plantId)) {
        saveFavoriteIds([...ids, plantId]);
      }

      return Promise.resolve();
    },

    remove(plantId) {
      saveFavoriteIds(readFavoriteIds().filter((id) => id !== plantId));
      return Promise.resolve();
    },

    ids() {
      return readFavoriteIds();
    },
  },

  recognition: {
    async identify(file, organ = "auto", scenario = "") {
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
          headers: {
            "X-User-Id": getOwnerId(),
          },
          body: formData,
        },
      );

      if (!response.ok) {
        throw new Error(`API error: ${response.status} ${response.statusText}`);
      }

      return response.json();
    },
  },
};
