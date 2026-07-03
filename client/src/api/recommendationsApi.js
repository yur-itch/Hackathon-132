import { apiRequest } from "./apiClient";

// plantIds — только для гостя: id растений его localStorage-коллекции,
// залогиненному коллекцию бэкенд берёт из БД сам.
export function getRecommendations(count = 3, plantIds = null) {
  const idsQuery = plantIds?.length ? `&plantIds=${plantIds.join(",")}` : "";
  return apiRequest(`/api/recommendations?count=${count}${idsQuery}`);
}
