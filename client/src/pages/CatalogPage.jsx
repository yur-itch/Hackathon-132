import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import PlantCard from "../components/PlantCard.jsx";

export default function CatalogPage() {
  const [search, setSearch] = useState("");
  const [plants, setPlants] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [favoriteIds, setFavoriteIds] = useState(api.favorites.ids());

  useEffect(() => {
    setLoading(true);
    setError("");

    api.plants
      .list({ search })
      .then(setPlants)
      .catch(() => setError("Не удалось загрузить справочник"))
      .finally(() => setLoading(false));
  }, [search]);

  function toggleFavorite(plantId) {
    if (favoriteIds.includes(plantId)) {
      api.favorites.remove(plantId);
    } else {
      api.favorites.add(plantId);
    }

    setFavoriteIds(api.favorites.ids());
  }

  return (
    <section>
      <div className="page-title">
        <h1>Справочник растений</h1>
        <p>Найдите растение и посмотрите основные рекомендации по уходу.</p>
      </div>

      <input
        className="input search-input"
        value={search}
        onChange={(event) => setSearch(event.target.value)}
        placeholder="Поиск по названию"
      />

      {loading && <p className="muted">Загрузка…</p>}
      {error && <p className="error">{error}</p>}

      <div className="card-grid">
        {plants.map((plant) => {
          const isFavorite = favoriteIds.includes(plant.id);

          return (
            <PlantCard
              key={plant.id}
              plant={plant}
              actions={
                <button
                  className={isFavorite ? "button button-secondary" : "button"}
                  onClick={() => toggleFavorite(plant.id)}
                >
                  {isFavorite ? "В избранном" : "В избранное"}
                </button>
              }
            />
          );
        })}
      </div>

      {!loading && !error && plants.length === 0 && (
        <p className="muted">Растения с таким названием не найдены.</p>
      )}
    </section>
  );
}
