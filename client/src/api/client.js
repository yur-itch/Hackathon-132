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

const FAVORITES_KEY = "favoritePlantIds";

function readFavoriteIds() {
  try {
    return JSON.parse(localStorage.getItem(FAVORITES_KEY)) || [];
  } catch {
    return [];
  }
}

function saveFavoriteIds(ids) {
  localStorage.setItem(FAVORITES_KEY, JSON.stringify(ids));
}

export const api = {
  plants: {
    list: getPlants,
    get: getPlantById,
  },

  userPlants: {
    listMine: getUserPlants,
    create: addUserPlant,
    update: updateUserPlant,
    remove: deleteUserPlant,
  },

  favorites: {
    listMine: getFavorites,
    add(plantId) {
      const ids = readFavoriteIds();

      if (!ids.includes(plantId)) {
        saveFavoriteIds([...ids, plantId]);
      }

      return addFavorite(plantId).catch(() => null);
    },
    remove(plantId) {
      saveFavoriteIds(readFavoriteIds().filter((id) => id !== plantId));
      return deleteFavorite(plantId).catch(() => null);
    },
    ids: readFavoriteIds,
  },

  reminders: {
    listMine: getReminders,
    create: createReminder,
    update: updateReminder,
    markDone: markReminderDone,
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
};
