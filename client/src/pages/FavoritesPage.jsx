import { useEffect, useState } from "react";
import { api } from "../api/client.js";

export default function FavoritesPage() {
  const [plants, setPlants] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    api.favorites
      .listMine()
      .then(setPlants)
      .catch(() => setError("Не удалось загрузить избранное"));
  }, []);

  return (
    <section>
      <div className="page-title">
        <h1>Избранное</h1>
        <p>Растения, которые пользователь сохранил для быстрого доступа.</p>
      </div>

      {error && <p className="error">{error}</p>}

      <div className="card-grid">
        {plants.map((plant) => (
          <article className="plant-card" key={plant.id}>
            <div className="plant-card-body">
              <h2>{plant.name}</h2>
              {plant.latinName && <p className="latin-name">{plant.latinName}</p>}
            </div>
          </article>
        ))}
      </div>

      {plants.length === 0 && !error && <p className="muted">В избранном пока пусто.</p>}
    </section>
  );
}
