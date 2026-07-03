// Владелец: Frontend-разработчик №2 (избранное — часть справочника).
import { useEffect, useState } from "react";
import { api } from "../api/apClient";
import type { Plant } from "../api/types";

export default function FavoritesPage() {
  const [plants, setPlants] = useState<Plant[]>([]);
  useEffect(() => {
    api.favorites.listMine().then(setPlants);
  }, []);

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {plants.map((p) => (
        <article key={p.id} className="rounded-lg border bg-white p-3">
          <h3 className="font-semibold">{p.name}</h3>
          <p className="text-sm italic text-gray-500">{p.latinName}</p>
        </article>
      ))}
      {plants.length === 0 && <p className="text-gray-500">В избранном пусто.</p>}
    </div>
  );
}
