import { useEffect, useState } from "react";
import { api } from "../api/client.js";

function getWateringText(plant) {
  return plant.wateringRecommendations || `раз в ${plant.wateringFrequencyDays} дней`;
}

function getLightText(plant) {
  return plant.lightingRecommendations || plant.light || "не указано";
}

export default function CatalogPage() {
  const [plants, setPlants] = useState([]);
  const [favoriteIds, setFavoriteIds] = useState(api.favorites.ids());
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    setLoading(true);
    setError("");

    api.plants
      .list({ search })
      .then(setPlants)
      .catch(() => setError("Не удалось загрузить растения"))
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
        <p>Найдите растение и посмотрите базовые рекомендации по уходу.</p>
      </div>

      <input
        className="input search-input"
        value={search}
        onChange={(event) => setSearch(event.target.value)}
        placeholder="Поиск растения"
      />

      {loading && <p className="muted">Загрузка...</p>}
      {error && <p className="error">{error}</p>}

      <div className="card-grid">
        {plants.map((plant) => (
          <article className="plant-card" key={plant.id}>
            {plant.imageUrl && (
              <img className="plant-image" src={plant.imageUrl} alt={plant.name} />
            )}

            <div className="plant-card-body">
              <h2>{plant.name}</h2>
              {plant.description && <p className="muted">{plant.description}</p>}
              <p>Полив: {getWateringText(plant)}</p>
              <p>Освещение: {getLightText(plant)}</p>
              <p>{plant.isPoisonous ? "Ядовитое растение" : "Не отмечено как ядовитое"}</p>

              <button className="button button-secondary" onClick={() => toggleFavorite(plant.id)}>
                {favoriteIds.includes(plant.id) ? "Убрать из избранного" : "В избранное"}
              </button>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}
