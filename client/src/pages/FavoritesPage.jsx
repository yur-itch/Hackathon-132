import { useEffect, useState } from "react";
import { api } from "../api/client.js";

export default function FavoritesPage() {
  const [plants, setPlants] = useState([]);
  const [error, setError] = useState("");

  function loadFavorites() {
    api.favorites
      .listMine()
      .then(setPlants)
      .catch(() => setError("Не удалось загрузить избранное"));
  }

  useEffect(() => {
    loadFavorites();
  }, []);

  function removeFavorite(plantId) {
    api.favorites.remove(plantId);
    loadFavorites();
  }

  return (
    <section>
      <div className="page-title">
        <h1>Избранное</h1>
        <p>Растения, сохраненные в браузере для быстрого доступа.</p>
      </div>

      {error && <p className="error">{error}</p>}

      <div className="card-grid">
        {plants.map((plant) => (
          <article className="plant-card" key={plant.id}>
            <div className="plant-card-body">
              <h2>{plant.name}</h2>
              {plant.description && <p className="muted">{plant.description}</p>}
              <button className="button button-danger" onClick={() => removeFavorite(plant.id)}>
                Удалить
              </button>
            </div>
          </article>
        ))}
      </div>

      {plants.length === 0 && !error && <p className="muted">В избранном пока пусто.</p>}
    </section>
  );
}
