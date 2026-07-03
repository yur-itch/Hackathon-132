// Тонкий типизированный клиент к бэкенду.
// База берётся из VITE_API_URL (.env), заголовок X-User-Id идентифицирует
// «владельца» коллекции (в базовой версии — "local", после авторизации — id юзера).

import type {
  Plant,
  UserPlant,
  Reminder,
  CreateUserPlantDto,
  CreateReminderDto,
} from "./types";

const BASE = import.meta.env.VITE_API_URL ?? "http://localhost:5071";

function ownerId(): string {
  return localStorage.getItem("ownerId") ?? "local";
}

async function req<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      "X-User-Id": ownerId(),
      ...(init?.headers ?? {}),
    },
  });
  if (!res.ok) throw new Error(`${res.status} ${res.statusText} — ${path}`);
  // 204 No Content
  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}

export const api = {
  // Справочник
  plants: {
    list: (params?: { search?: string; difficulty?: string }) => {
      const qs = new URLSearchParams(
        Object.entries(params ?? {}).filter(([, v]) => v) as [string, string][],
      ).toString();
      return req<Plant[]>(`/api/plants${qs ? `?${qs}` : ""}`);
    },
    get: (id: number) => req<Plant>(`/api/plants/${id}`),
  },

  // Личная коллекция
  userPlants: {
    listMine: () => req<UserPlant[]>(`/api/userplants`),
    get: (id: number) => req<UserPlant>(`/api/userplants/${id}`),
    create: (dto: CreateUserPlantDto) =>
      req<UserPlant>(`/api/userplants`, { method: "POST", body: JSON.stringify(dto) }),
    remove: (id: number) => req<void>(`/api/userplants/${id}`, { method: "DELETE" }),
  },

  // Напоминания
  reminders: {
    listMine: (due = false) => req<Reminder[]>(`/api/reminders?due=${due}`),
    create: (dto: CreateReminderDto) =>
      req<Reminder>(`/api/reminders`, { method: "POST", body: JSON.stringify(dto) }),
    markDone: (id: number) => req<void>(`/api/reminders/${id}/done`, { method: "POST" }),
    remove: (id: number) => req<void>(`/api/reminders/${id}`, { method: "DELETE" }),
  },

  // Избранное
  favorites: {
    listMine: () => req<Plant[]>(`/api/favorites`),
    add: (plantId: number) => req<void>(`/api/favorites/${plantId}`, { method: "POST" }),
    remove: (plantId: number) => req<void>(`/api/favorites/${plantId}`, { method: "DELETE" }),
  },
};
