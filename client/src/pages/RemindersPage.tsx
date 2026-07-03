// Владелец: Frontend-разработчик №3 (напоминания).
// Экран: список напоминаний, «выполнено» сдвигает срок.
import { useEffect, useState } from "react";
import { api } from "../api/client";
import type { Reminder } from "../api/types";

const label: Record<Reminder["type"], string> = {
  Watering: "💧 Полив",
  Repotting: "🪴 Пересадка",
  Fertilizing: "🌱 Подкормка",
};

export default function RemindersPage() {
  const [items, setItems] = useState<Reminder[]>([]);
  const load = () => api.reminders.listMine().then(setItems);
  useEffect(() => {
    load();
  }, []);

  return (
    <ul className="space-y-2">
      {items.map((r) => {
        const due = new Date(r.nextDueAt) <= new Date();
        return (
          <li key={r.id} className="flex items-center justify-between rounded-lg border bg-white p-3">
            <div>
              <p className="font-medium">{label[r.type]}</p>
              <p className={`text-sm ${due ? "text-red-600" : "text-gray-500"}`}>
                {due ? "Пора!" : "Срок: "} {new Date(r.nextDueAt).toLocaleDateString()}
              </p>
            </div>
            <button
              onClick={() => api.reminders.markDone(r.id).then(load)}
              className="rounded-md bg-green-600 px-3 py-1.5 text-sm text-white"
            >
              Выполнено
            </button>
          </li>
        );
      })}
      {items.length === 0 && <p className="text-gray-500">Напоминаний нет.</p>}
    </ul>
  );
}
