import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import PlantCard from "../components/PlantCard.jsx";

export default function FavoritesPage() {
  const [favoritePlants, setFavoritePlants] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  function loadFavorites() {
    setLoading(true);
    setError("");

    api.favorites
      .listMine()
      .then(setFavoritePlants)
      .catch(() => setError("Не удалось загрузить избранное"))
      .finally(() => setLoading(false));
  }

  useEffect(loadFavorites, []);

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

      {loading && <p className="muted">Загрузка…</p>}
      {error && <p className="error">{error}</p>}

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

      {!loading && !error && favoritePlants.length === 0 && (
        <p className="muted">В избранном пока пусто.</p>
      )}
    </section>
  );
}
