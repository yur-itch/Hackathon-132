// Владелец: Frontend-разработчик №2 (справочник).
// Экран: список растений из справочника + поиск/фильтр + карточка.
import { useEffect, useState } from "react";
import { api } from "../api/client";
import type { Plant } from "../api/types";

export default function CatalogPage() {
  const [plants, setPlants] = useState<Plant[]>([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    api.plants
      .list({ search })
      .then(setPlants)
      .finally(() => setLoading(false));
  }, [search]);

  return (
    <div>
      <input
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        placeholder="Поиск растения…"
        className="mb-4 w-full rounded-md border px-3 py-2"
      />

      {loading && <p className="text-gray-500">Загрузка…</p>}

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {plants.map((p) => (
          <article key={p.id} className="overflow-hidden rounded-lg border bg-white">
            {p.imageUrl && (
              <img src={p.imageUrl} alt={p.name} className="h-40 w-full object-cover" />
            )}
            <div className="p-3">
              <h3 className="font-semibold">{p.name}</h3>
              <p className="text-sm italic text-gray-500">{p.latinName}</p>
              <p className="mt-2 text-sm">💧 полив раз в {p.wateringFrequencyDays} дн.</p>
              <p className="text-sm">☀️ {p.light}</p>
              <span className="mt-2 inline-block rounded bg-green-100 px-2 py-0.5 text-xs text-green-800">
                {p.difficulty}
              </span>
            </div>
          </article>
        ))}
      </div>
    </div>
  );
}
