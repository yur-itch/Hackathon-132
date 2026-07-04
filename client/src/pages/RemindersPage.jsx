import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import { useAuth } from "../context/AuthContext.jsx";
import {
  disablePushNotifications,
  enablePushNotifications,
  getSubscriptionState,
  isPushSupported,
} from "../pushNotifications.js";

// Тип напоминания с бэка/guestStore ("Watering"/…) → заголовок для UI.
const REMINDER_TITLES = {
  Watering: "Полив",
  Repotting: "Пересадка",
  Fertilizing: "Подкормка",
};

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
  const { user } = useAuth();
  const [reminders, setReminders] = useState([]);
  const [error, setError] = useState("");
  const [doneId, setDoneId] = useState(null);

  function load() {
    api.reminders
      .listMine()
      .then(setReminders)
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
        <p>Задачи по уходу за растениями из вашего списка.</p>
      </div>

      {user ? (
        <PushToggle />
      ) : (
        <p className="muted">Push-уведомления доступны после входа в аккаунт.</p>
      )}

      {error && <p className="error">{error}</p>}

      <div className="list">
        {reminders.map((reminder) => {
          const dueDate = new Date(reminder.nextDueAt);
          const isOverdue = dueDate <= new Date();

          return (
            <div className="list-item" key={reminder.id}>
              <div>
                <h2>{REMINDER_TITLES[reminder.type] ?? "Уход"}</h2>
                <p className="muted">{reminder.plantName}</p>
                <p className={isOverdue ? "error" : "muted"}>
                  {isOverdue ? "Пора выполнить" : "Срок"}: {dueDate.toLocaleDateString()}
                </p>
              </div>

              <button
                className="button"
                onClick={() => markDone(reminder.id)}
                disabled={doneId === reminder.id || !isOverdue}
                title={isOverdue ? undefined : "Срок ещё не наступил"}
              >
                {doneId === reminder.id ? "Подождите…" : "Готово"}
              </button>
            </div>
          );
        })}
      </div>

      {reminders.length === 0 && !error && <p className="muted">Напоминаний пока нет.</p>}
    </section>
  );
}
