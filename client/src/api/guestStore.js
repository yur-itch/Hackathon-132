// Гостевой режим: коллекция, избранное и даты ухода живут в localStorage браузера,
// пока пользователь не создал аккаунт (базовый сценарий из ТЗ — без регистрации).
// Формы записей повторяют DTO бэкенда, чтобы страницы не отличали гостя от залогиненного.
// Все методы асинхронные — тот же контракт, что у серверных модулей.
import { getPlants } from "./plantsApi.js";

const FAVORITES_KEY = "plantcare.guest.favoriteIds";
const PLANTS_KEY = "plantcare.guest.plants";

function read(key) {
  try {
    return JSON.parse(localStorage.getItem(key));
  } catch {
    return null;
  }
}

function write(key, value) {
  localStorage.setItem(key, JSON.stringify(value));
}

function inDays(days) {
  return new Date(Date.now() + days * 24 * 60 * 60 * 1000).toISOString();
}

export const favorites = {
  async ids() {
    return read(FAVORITES_KEY) ?? [];
  },

  async list() {
    const ids = read(FAVORITES_KEY) ?? [];
    const plants = await getPlants();
    return plants.filter((plant) => ids.includes(plant.id));
  },

  async add(plantId) {
    const ids = read(FAVORITES_KEY) ?? [];
    if (!ids.includes(plantId)) {
      write(FAVORITES_KEY, [...ids, plantId]);
    }
  },

  async remove(plantId) {
    write(FAVORITES_KEY, (read(FAVORITES_KEY) ?? []).filter((id) => id !== plantId));
  },
};

export const userPlants = {
  async list() {
    return read(PLANTS_KEY) ?? [];
  },

  // Повторяет логику бэкенда: дефолтные даты = сегодня + интервал из справочника,
  // напоминание о пересадке — только если у растения есть интервал или дана дата.
  async create({ plantId, note, nextWateringDate, nextRepottingDate }) {
    const records = read(PLANTS_KEY) ?? [];
    if (records.some((record) => record.plantId === plantId)) {
      throw new Error("Plant is already added to the collection.");
    }

    const plants = await getPlants();
    const plant = plants.find((item) => item.id === plantId);
    if (!plant) {
      throw new Error("Растение не найдено в справочнике.");
    }

    const wateringIntervalDays =
      plant.wateringFrequencyDays > 0 ? plant.wateringFrequencyDays : 7;
    const repottingIntervalDays = plant.repottingFrequencyMonths
      ? plant.repottingFrequencyMonths * 30
      : null;

    const repottingDate =
      nextRepottingDate ||
      (repottingIntervalDays ? inDays(repottingIntervalDays) : null);

    const id = crypto.randomUUID();
    const record = {
      id,
      plantId,
      plantName: plant.name,
      plantImageUrl: plant.imageUrl,
      note: note ?? null,
      addedAt: new Date().toISOString(),
      nextWateringDate: nextWateringDate || inDays(wateringIntervalDays),
      nextRepottingDate: repottingDate,
      wateringReminderId: `${id}:watering`,
      repottingReminderId: repottingDate ? `${id}:repotting` : null,
      wateringIntervalDays,
      repottingIntervalDays: repottingIntervalDays ?? 365,
    };

    write(PLANTS_KEY, [record, ...records]);
    return record;
  },

  async remove(id) {
    write(PLANTS_KEY, (read(PLANTS_KEY) ?? []).filter((record) => record.id !== id));
  },
};

export const reminders = {
  // Готовый список напоминаний из локальной коллекции — той же формы, что ReminderDto
  // бэкенда (id, type, plantName, nextDueAt), чтобы страница не отличала гостя.
  async list() {
    const records = read(PLANTS_KEY) ?? [];
    const items = [];

    for (const record of records) {
      if (record.nextWateringDate) {
        items.push({
          id: `${record.id}:watering`,
          type: "Watering",
          plantName: record.plantName,
          nextDueAt: record.nextWateringDate,
        });
      }
      if (record.nextRepottingDate) {
        items.push({
          id: `${record.id}:repotting`,
          type: "Repotting",
          plantName: record.plantName,
          nextDueAt: record.nextRepottingDate,
        });
      }
    }

    items.sort((a, b) => new Date(a.nextDueAt) - new Date(b.nextDueAt));
    return items;
  },

  // Гостевой reminderId — "<id записи>:watering|repotting".
  // «Готово» сдвигает срок на интервал вперёд, как на бэке.
  async markDone(reminderId) {
    const [recordId, type] = String(reminderId).split(":");
    const records = read(PLANTS_KEY) ?? [];
    const record = records.find((item) => item.id === recordId);
    if (!record) {
      throw new Error("Напоминание не найдено.");
    }

    if (type === "repotting") {
      record.nextRepottingDate = inDays(record.repottingIntervalDays ?? 365);
    } else {
      record.nextWateringDate = inDays(record.wateringIntervalDays ?? 7);
    }

    write(PLANTS_KEY, records);
  },
};

export function hasGuestData() {
  return (read(FAVORITES_KEY) ?? []).length > 0 || (read(PLANTS_KEY) ?? []).length > 0;
}

export function exportGuestData() {
  return {
    favoriteIds: read(FAVORITES_KEY) ?? [],
    plants: read(PLANTS_KEY) ?? [],
  };
}

export function clearGuestData() {
  localStorage.removeItem(FAVORITES_KEY);
  localStorage.removeItem(PLANTS_KEY);
}
