import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import PlantCard from "../components/PlantCard.jsx";

export default function CatalogPage() {
  const [plants, setPlants] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [favoriteIds, setFavoriteIds] = useState([]);
  const [recommended, setRecommended] = useState([]);

  useEffect(() => {
    setLoading(true);
    setError("");

    api.plants
      .list()
      .then(setPlants)
      .catch(() => setError("Не удалось загрузить справочник"))
      .finally(() => setLoading(false));

    api.favorites.ids().then(setFavoriteIds).catch(() => {});

    // Рекомендации — необязательное украшение: при ошибке просто не показываем секцию
    api.recommendations.list(3).then(setRecommended).catch(() => {});
  }, []);

  async function toggleFavorite(plantId) {
    const isFavorite = favoriteIds.includes(plantId);

    // Оптимистично меняем кнопку, при ошибке перечитываем состояние с бэка
    setFavoriteIds((current) =>
      isFavorite ? current.filter((id) => id !== plantId) : [...current, plantId],
    );

    try {
      if (isFavorite) {
        await api.favorites.remove(plantId);
      } else {
        await api.favorites.add(plantId);
      }
    } catch {
      api.favorites.ids().then(setFavoriteIds).catch(() => {});
    }
  }

  function favoriteButton(plantId) {
    const isFavorite = favoriteIds.includes(plantId);

    return (
      <button
        className={isFavorite ? "button button-secondary" : "button"}
        onClick={() => toggleFavorite(plantId)}
      >
        {isFavorite ? "В избранном" : "В избранное"}
      </button>
    );
  }

  return (
    <section>
      <div className="page-title">
        <h1>Справочник растений</h1>
        <p>Посмотрите основные рекомендации по уходу за комнатными растениями.</p>
      </div>

      {loading && <p className="muted">Загрузка…</p>}
      {error && <p className="error">{error}</p>}

      {recommended.length > 0 && (
        <>
          <h2>Вам может подойти</h2>
          <p className="muted">Подборка на основе вашей коллекции.</p>
          <div className="card-grid">
            {recommended.map((plant) => (
              <PlantCard key={plant.id} plant={plant} actions={favoriteButton(plant.id)} />
            ))}
          </div>
          <h2>Все растения</h2>
        </>
      )}

      <div className="card-grid">
        {plants.map((plant) => (
          <PlantCard key={plant.id} plant={plant} actions={favoriteButton(plant.id)} />
        ))}
      </div>

      {!loading && !error && plants.length === 0 && (
        <p className="muted">В справочнике пока нет растений.</p>
      )}
    </section>
  );
}
