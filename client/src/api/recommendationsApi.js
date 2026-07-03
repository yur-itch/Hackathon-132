import { apiRequest } from "./apiClient";

export function getRecommendations(count = 3) {
  return apiRequest(`/api/recommendations?count=${count}`);
}
