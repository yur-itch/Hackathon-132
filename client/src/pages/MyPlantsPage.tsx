// Владелец: Frontend-разработчик №3 (личная коллекция).
// Экран: список «моих растений», добавление/удаление.
import { useEffect, useState } from "react";
import { api } from "../api/client";
import type { UserPlant } from "../api/types";

export default function MyPlantsPage() {
  const [items, setItems] = useState<UserPlant[]>([]);
  const [nickname, setNickname] = useState("");

  const load = () => api.userPlants.listMine().then(setItems);
  useEffect(() => {
    load();
  }, []);

  const add = async () => {
    if (!nickname.trim()) return;
    await api.userPlants.create({ nickname });
    setNickname("");
    load();
  };

  return (
    <div>
      <div className="mb-4 flex gap-2">
        <input
          value={nickname}
          onChange={(e) => setNickname(e.target.value)}
          placeholder="Название растения"
          className="flex-1 rounded-md border px-3 py-2"
        />
        <button onClick={add} className="rounded-md bg-green-600 px-4 py-2 text-white">
          Добавить
        </button>
      </div>

      <ul className="space-y-2">
        {items.map((up) => (
          <li key={up.id} className="flex items-center justify-between rounded-lg border bg-white p-3">
            <div>
              <p className="font-medium">{up.nickname}</p>
              {up.location && <p className="text-sm text-gray-500">{up.location}</p>}
            </div>
            <button
              onClick={() => api.userPlants.remove(up.id).then(load)}
              className="text-sm text-red-600"
            >
              Удалить
            </button>
          </li>
        ))}
        {items.length === 0 && <p className="text-gray-500">Пока пусто. Добавьте растение.</p>}
      </ul>
    </div>
  );
}
