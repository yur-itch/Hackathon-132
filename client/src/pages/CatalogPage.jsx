import { useEffect, useState } from "react";
import { api } from "../api/client.js";

export default function CatalogPage() {
  const [plants, setPlants] = useState([]);
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
              {plant.latinName && <p className="latin-name">{plant.latinName}</p>}
              <p>Полив: раз в {plant.wateringFrequencyDays} дней</p>
              <p>Освещение: {plant.light}</p>
              <p>Сложность: {plant.difficulty}</p>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}
