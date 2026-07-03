import { apiRequest } from "./apiClient";

export function getExchangeOffers() {
  return apiRequest("/api/exchange/offers");
}

export function getExchangeOfferById(id) {
  return apiRequest(`/api/exchange/offers/${id}`);
}

export function createExchangeOffer(data) {
  return apiRequest("/api/exchange/offers", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export function closeExchangeOffer(id) {
  return apiRequest(`/api/exchange/offers/${id}`, {
    method: "DELETE",
  });
}

export function sendExchangeMessage(offerId, data) {
  return apiRequest(`/api/exchange/offers/${offerId}/messages`, {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export function getExchangeMessages(offerId, otherUserId) {
  const query = `?otherUserId=${encodeURIComponent(otherUserId)}`;
  return apiRequest(`/api/exchange/offers/${offerId}/messages${query}`);
}

export function getExchangeChats() {
  return apiRequest("/api/exchange/chats");
}

export function confirmExchange(offerId, data) {
  return apiRequest(`/api/exchange/offers/${offerId}/confirm`, {
    method: "POST",
    body: JSON.stringify(data),
  });
}
