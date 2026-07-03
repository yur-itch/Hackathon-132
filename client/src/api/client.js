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
  },

  reminders: {
    listMine: getReminders,
    create: createReminder,
    update: updateReminder,
    markDone: markReminderDone,
    remove: deleteReminder,
  },
};
