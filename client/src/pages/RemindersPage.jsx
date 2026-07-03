import { useEffect, useState } from "react";
import { api } from "../api/client.js";

function buildReminders(plants) {
  return plants.flatMap((plant) => {
    const items = [];

    if (plant.nextWateringDate) {
      items.push({
        id: `${plant.id}-water`,
        title: "Полив",
        plantName: plant.plantName,
        date: plant.nextWateringDate,
      });
    }

    if (plant.nextRepottingDate) {
      items.push({
        id: `${plant.id}-repot`,
        title: "Пересадка",
        plantName: plant.plantName,
        date: plant.nextRepottingDate,
      });
    }

    return items;
  });
}

export default function RemindersPage() {
  const [reminders, setReminders] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    api.userPlants
      .listMine()
      .then((plants) => setReminders(buildReminders(plants)))
      .catch(() => setError("Не удалось загрузить напоминания"));
  }, []);

  return (
    <section>
      <div className="page-title">
        <h1>Напоминания</h1>
        <p>Задачи строятся по датам, указанным в личном списке растений.</p>
      </div>

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
            </div>
          );
        })}
      </div>

      {reminders.length === 0 && !error && <p className="muted">Напоминаний пока нет.</p>}
    </section>
  );
}
