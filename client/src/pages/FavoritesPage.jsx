import { useState } from "react";
import { api } from "../api/client.js";
import { demoPlants } from "../data/demoPlants.js";
import PlantCard from "../components/PlantCard.jsx";

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
          <PlantCard
            key={plant.id}
            plant={plant}
            actions={
              <button className="button button-danger" onClick={() => removeFavorite(plant.id)}>
                Убрать из избранного
              </button>
            }
          />
        ))}
      </div>

      {favoritePlants.length === 0 && <p className="muted">В избранном пока пусто.</p>}
    </section>
  );
}
