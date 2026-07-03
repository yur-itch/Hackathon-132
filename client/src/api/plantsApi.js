import { apiRequest } from "./apiClient";

export function getPlants(search = "") { // Если поиск не передали, то просто пустая строка пофик
  const query = search
    ? `?search=${encodeURIComponent(search)}`
    : "";

  return apiRequest(`/api/plants${query}`);
}

export function getPlantById(id) {
  return apiRequest(`/api/plants/${id}`);
}
