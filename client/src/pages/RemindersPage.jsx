import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import {
  disablePushNotifications,
  enablePushNotifications,
  getSubscriptionState,
  isPushSupported,
} from "../pushNotifications.js";

function buildReminders(plants) {
  return plants.flatMap((plant) => {
    const items = [];

    if (plant.nextWateringDate) {
      items.push({
        id: `${plant.id}-water`,
        reminderId: plant.wateringReminderId,
        title: "Полив",
        plantName: plant.plantName,
        date: plant.nextWateringDate,
      });
    }

    if (plant.nextRepottingDate) {
      items.push({
        id: `${plant.id}-repot`,
        reminderId: plant.repottingReminderId,
        title: "Пересадка",
        plantName: plant.plantName,
        date: plant.nextRepottingDate,
      });
    }

    return items;
  });
}

function PushToggle() {
  const [supported, setSupported] = useState(true);
  const [subscribed, setSubscribed] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    getSubscriptionState().then((state) => {
      setSupported(state.supported);
      setSubscribed(state.subscribed);
    });
  }, []);

  async function toggle() {
    setLoading(true);
    setError("");

    try {
      if (subscribed) {
        await disablePushNotifications();
        setSubscribed(false);
      } else {
        await enablePushNotifications();
        setSubscribed(true);
      }
    } catch (toggleError) {
      setError(toggleError.message || "Не удалось изменить настройку уведомлений");
    } finally {
      setLoading(false);
    }
  }

  if (!isPushSupported() || !supported) {
    return null;
  }

  return (
    <div className="form-panel narrow">
      <button className={subscribed ? "button button-secondary" : "button"} onClick={toggle} disabled={loading}>
        {loading
          ? "Подождите…"
          : subscribed
            ? "Уведомления включены — выключить"
            : "Включить push-уведомления"}
      </button>
      {error && <p className="error">{error}</p>}
    </div>
  );
}

export default function RemindersPage() {
  const [reminders, setReminders] = useState([]);
  const [error, setError] = useState("");
  const [doneId, setDoneId] = useState(null);

  function load() {
    api.userPlants
      .listMine()
      .then((plants) => setReminders(buildReminders(plants)))
      .catch(() => setError("Не удалось загрузить напоминания"));
  }

  useEffect(load, []);

  async function markDone(reminderId) {
    setDoneId(reminderId);
    setError("");

    try {
      await api.reminders.markDone(reminderId);
      load();
    } catch {
      setError("Не удалось отметить напоминание выполненным");
    } finally {
      setDoneId(null);
    }
  }

  return (
    <section>
      <div className="page-title">
        <h1>Напоминания</h1>
        <p>Задачи строятся по датам, указанным в личном списке растений.</p>
      </div>

      <PushToggle />

      {error && <p className="error">{error}</p>}

      <div className="list">
        {reminders.map((reminder) => {
          const dueDate = new Date(reminder.date);
          const isOverdue = dueDate <= new Date();

          return (
            <div className="list-item" key={reminder.id}>
              <div>
                <h2>{reminder.title}</h2>
                <p className="muted">{reminder.plantName}</p>
                <p className={isOverdue ? "error" : "muted"}>
                  {isOverdue ? "Пора выполнить" : "Срок"}: {dueDate.toLocaleDateString()}
                </p>
              </div>

              {reminder.reminderId && (
                <button
                  className="button"
                  onClick={() => markDone(reminder.reminderId)}
                  disabled={doneId === reminder.reminderId}
                >
                  {doneId === reminder.reminderId ? "Подождите…" : "Готово"}
                </button>
              )}
            </div>
          );
        })}
      </div>

      {reminders.length === 0 && !error && <p className="muted">Напоминаний пока нет.</p>}
    </section>
  );
}
