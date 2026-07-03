import { apiRequest } from "./apiClient";

export function getReminders(dueOnly = false) {
  return apiRequest(`/api/reminders?dueOnly=${dueOnly}`);
}

export function createReminder(data) {
  return apiRequest("/api/reminders", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export function updateReminder(id, data) {
  return apiRequest(`/api/reminders/${id}`, {
    method: "PUT",
    body: JSON.stringify(data),
  });
}

export function markReminderDone(id) {
  return apiRequest(`/api/reminders/${id}/done`, {
    method: "POST",
  });
}

export function deleteReminder(id) {
  return apiRequest(`/api/reminders/${id}`, {
    method: "DELETE",
  });
}
