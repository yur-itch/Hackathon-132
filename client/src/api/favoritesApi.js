import { apiRequest } from "./apiClient";

export function getFavorites() {
  return apiRequest("/api/favorites");
}

export function addFavorite(plantId) {
  return apiRequest(`/api/favorites/${plantId}`, {
    method: "POST",
  });
}

export function deleteFavorite(plantId) {
  return apiRequest(`/api/favorites/${plantId}`, {
    method: "DELETE",
  });
}
