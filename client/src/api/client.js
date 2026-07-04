import { addFavorite, deleteFavorite, getFavorites } from "./favoritesApi.js";
import { getPlantById, getPlants } from "./plantsApi.js";
import {
  createReminder,
  deleteReminder,
  getReminders,
  markReminderDone,
  updateReminder,
} from "./remindersApi.js";
import {
  addUserPlant,
  deleteUserPlant,
  getUserPlants,
  updateUserPlant,
} from "./userPlantsApi.js";
import { identify } from "./recognitionApi.js";
import { getMe, login, logout, register } from "./authApi.js";
import { getVapidPublicKey, subscribe, unsubscribe } from "./pushApi.js";
import {
  closeExchangeOffer,
  confirmExchange,
  createExchangeOffer,
  getExchangeChats,
  getExchangeMessages,
  getExchangeOfferById,
  getExchangeOffers,
  sendExchangeMessage,
} from "./exchangeApi.js";
import { getRecommendations } from "./recommendationsApi.js";
import * as guestStore from "./guestStore.js";

// Гостевой режим: без аккаунта коллекция/избранное/напоминания работают через
// localStorage (guestStore), после входа — через бэкенд. Флаг ставит AuthContext.
let guestMode = false;

export function setGuestMode(value) {
  guestMode = Boolean(value);
}

// Переносим гостевые данные в аккаунт после логина/регистрации.
// Конфликты (уже добавлено) — штатный случай; localStorage чистим только если
// не было неожиданных ошибок, чтобы не потерять данные при сбое сети.
async function importGuestDataToAccount() {
  const { favoriteIds, plants } = guestStore.exportGuestData();
  if (favoriteIds.length === 0 && plants.length === 0) {
    return;
  }

  let unexpectedError = false;

  for (const plantId of favoriteIds) {
    try {
      await addFavorite(plantId);
    } catch (error) {
      if (!(error.message || "").includes("already")) unexpectedError = true;
    }
  }

  for (const record of plants) {
    try {
      await addUserPlant({
        plantId: record.plantId,
        note: record.note,
        nextWateringDate: record.nextWateringDate,
        nextRepottingDate: record.nextRepottingDate,
      });
    } catch (error) {
      if (!(error.message || "").includes("already")) unexpectedError = true;
    }
  }

  if (!unexpectedError) {
    guestStore.clearGuestData();
  }
}

export const api = {
  plants: {
    list: getPlants,
    get: getPlantById,
  },

  userPlants: {
    listMine: () => (guestMode ? guestStore.userPlants.list() : getUserPlants()),
    create: (data) => (guestMode ? guestStore.userPlants.create(data) : addUserPlant(data)),
    update: updateUserPlant, // гостевой режим не редактирует записи (страницы этим не пользуются)
    remove: (id) => (guestMode ? guestStore.userPlants.remove(id) : deleteUserPlant(id)),
  },

  favorites: {
    listMine: () => (guestMode ? guestStore.favorites.list() : getFavorites()),
    add: (plantId) => (guestMode ? guestStore.favorites.add(plantId) : addFavorite(plantId)),
    remove: (plantId) =>
      guestMode ? guestStore.favorites.remove(plantId) : deleteFavorite(plantId),
    // Единый источник правды: бэкенд для залогиненных, guestStore для гостя.
    ids: () =>
      guestMode
        ? guestStore.favorites.ids()
        : getFavorites().then((plants) => plants.map((plant) => plant.id)),
  },

  reminders: {
    listMine: () => (guestMode ? guestStore.reminders.list() : getReminders()),
    create: createReminder,
    update: updateReminder,
    markDone: (id) => (guestMode ? guestStore.reminders.markDone(id) : markReminderDone(id)),
    remove: deleteReminder,
  },

  recognition: {
    identify,
  },

  auth: {
    register,
    login,
    logout,
    me: getMe,
  },

  push: {
    vapidPublicKey: getVapidPublicKey,
    subscribe,
    unsubscribe,
  },

  exchange: {
    listOffers: getExchangeOffers,
    getOffer: getExchangeOfferById,
    createOffer: createExchangeOffer,
    closeOffer: closeExchangeOffer,
    sendMessage: sendExchangeMessage,
    getMessages: getExchangeMessages,
    listChats: getExchangeChats,
    confirm: confirmExchange,
  },

  recommendations: {
    async list(count = 3) {
      if (!guestMode) {
        return getRecommendations(count);
      }

      const plants = await guestStore.userPlants.list();
      return getRecommendations(count, plants.map((record) => record.plantId));
    },
  },

  guest: {
    hasData: guestStore.hasGuestData,
    importToAccount: importGuestDataToAccount,
  },
};
