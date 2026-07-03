import { apiRequest } from "./apiClient";

export function getUserPlants() {
  return apiRequest("/api/user-plants");
}

export function addUserPlant(data) {
  return apiRequest("/api/user-plants", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export function updateUserPlant(id, data) {
  return apiRequest(`/api/user-plants/${id}`, {
    method: "PUT",
    body: JSON.stringify(data),
  });
}

export function deleteUserPlant(id) {
  return apiRequest(`/api/user-plants/${id}`, {
    method: "DELETE",
  });
}
