import { useEffect, useState } from "react";
import { api } from "../api/client.js";

export default function MyPlantsPage() {
  const [plants, setPlants] = useState([]);
  const [nickname, setNickname] = useState("");
  const [error, setError] = useState("");

  function loadPlants() {
    api.userPlants
      .listMine()
      .then(setPlants)
      .catch(() => setError("Не удалось загрузить личный список"));
  }

  useEffect(() => {
    loadPlants();
  }, []);

  async function addPlant() {
    const name = nickname.trim();

    if (!name) {
      return;
    }

    await api.userPlants.create({ nickname: name });
    setNickname("");
    loadPlants();
  }

  return (
    <section>
      <div className="page-title">
        <h1>Мои растения</h1>
        <p>Личный список растений пользователя.</p>
      </div>

      <div className="form-row">
        <input
          className="input"
          value={nickname}
          onChange={(event) => setNickname(event.target.value)}
          placeholder="Название растения"
        />
        <button className="button" onClick={addPlant}>
          Добавить
        </button>
      </div>

      {error && <p className="error">{error}</p>}

      <div className="list">
        {plants.map((plant) => (
          <div className="list-item" key={plant.id}>
            <div>
              <h2>{plant.nickname}</h2>
              {plant.location && <p className="muted">{plant.location}</p>}
            </div>

            <button
              className="button button-danger"
              onClick={() => api.userPlants.remove(plant.id).then(loadPlants)}
            >
              Удалить
            </button>
          </div>
        ))}
      </div>

      {plants.length === 0 && !error && <p className="muted">Список пока пуст.</p>}
    </section>
  );
}
