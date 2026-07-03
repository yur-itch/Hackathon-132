import { apiRequest } from "./apiClient";

export function getPlants() {
  return apiRequest("/api/plants");
}

export function getPlantById(id) {
  return apiRequest(`/api/plants/${id}`);
}
