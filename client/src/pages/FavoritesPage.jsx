import { useState } from "react";
import { api } from "../api/client.js";
import { demoPlants } from "../data/demoPlants.js";

export default function FavoritesPage() {
  const [favoriteIds, setFavoriteIds] = useState(api.favorites.ids());

  const favoritePlants = demoPlants.filter((plant) => favoriteIds.includes(plant.id));

  function removeFavorite(plantId) {
    api.favorites.remove(plantId);
    setFavoriteIds(api.favorites.ids());
  }

  return (
    <section>
      <div className="page-title">
        <h1>Избранное</h1>
        <p>Растения, сохраненные в браузере для быстрого доступа.</p>
      </div>

      <div className="card-grid">
        {favoritePlants.map((plant) => (
          <article className="plant-card" key={plant.id}>
            <div className="plant-card-body">
              <h2>{plant.name}</h2>
              <p className="muted">{plant.description}</p>

              <div className="plant-facts">
                <p>
                  <strong>Полив:</strong> {plant.watering}
                </p>
                <p>
                  <strong>Освещение:</strong> {plant.light}
                </p>
                <p>
                  <strong>Пересадка:</strong> {plant.repotting}
                </p>
                <p>
                  <strong>Ядовитость:</strong> {plant.toxicity}
                </p>
                <p>
                  <strong>Сложность ухода:</strong> {plant.difficulty}
                </p>
              </div>

              <button className="button button-danger" onClick={() => removeFavorite(plant.id)}>
                Убрать из избранного
              </button>
            </div>
          </article>
        ))}
      </div>

      {favoritePlants.length === 0 && <p className="muted">В избранном пока пусто.</p>}
    </section>
  );
}
