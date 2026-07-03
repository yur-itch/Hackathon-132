import { useEffect, useState } from "react";
import { api } from "../api/client.js";

const reminderLabels = {
  Watering: "Полив",
  Repotting: "Пересадка",
  Fertilizing: "Подкормка",
};

export default function RemindersPage() {
  const [reminders, setReminders] = useState([]);
  const [error, setError] = useState("");

  function loadReminders() {
    api.reminders
      .listMine()
      .then(setReminders)
      .catch(() => setError("Не удалось загрузить напоминания"));
  }

  useEffect(() => {
    loadReminders();
  }, []);

  return (
    <section>
      <div className="page-title">
        <h1>Напоминания</h1>
        <p>Список задач по уходу за растениями.</p>
      </div>

      {error && <p className="error">{error}</p>}

      <div className="list">
        {reminders.map((reminder) => {
          const dueDate = new Date(reminder.nextDueAt);
          const isOverdue = dueDate <= new Date();

          return (
            <div className="list-item" key={reminder.id}>
              <div>
                <h2>{reminderLabels[reminder.type] || reminder.type}</h2>
                <p className={isOverdue ? "error" : "muted"}>
                  {isOverdue ? "Пора выполнить" : "Срок"}: {dueDate.toLocaleDateString()}
                </p>
              </div>

              <button
                className="button"
                onClick={() => api.reminders.markDone(reminder.id).then(loadReminders)}
              >
                Готово
              </button>
            </div>
          );
        })}
      </div>

      {reminders.length === 0 && !error && <p className="muted">Напоминаний пока нет.</p>}
    </section>
  );
}
