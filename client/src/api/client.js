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
    add: addFavorite,
    remove: deleteFavorite,
    // Единый источник правды — бэкенд; все методы асинхронные.
    ids: () => getFavorites().then((plants) => plants.map((plant) => plant.id)),
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

  recommendations: {
    list: getRecommendations,
  },
};
