import { apiRequest } from "./apiClient";

export function getVapidPublicKey() {
  return apiRequest("/api/push/vapid-public-key");
}

export function subscribe(subscription) {
  return apiRequest("/api/push/subscribe", {
    method: "POST",
    body: JSON.stringify(subscription),
  });
}

export function unsubscribe(endpoint) {
  return apiRequest("/api/push/unsubscribe", {
    method: "POST",
    body: JSON.stringify({ endpoint }),
  });
}
