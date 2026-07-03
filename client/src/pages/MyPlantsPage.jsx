import { useEffect, useState } from "react";
import { api } from "../api/client.js";

export default function MyPlantsPage() {
  const [catalog, setCatalog] = useState([]);
  const [plants, setPlants] = useState([]);
  const [form, setForm] = useState({
    plantId: "",
    note: "",
    nextWateringDate: "",
    nextRepottingDate: "",
  });
  const [error, setError] = useState("");

  function loadUserPlants() {
    api.userPlants
      .listMine()
      .then(setPlants)
      .catch(() => setError("Не удалось загрузить личный список"));
  }

  useEffect(() => {
    api.plants.list().then(setCatalog).catch(() => setError("Не удалось загрузить справочник"));
    loadUserPlants();
  }, []);

  function updateField(field, value) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  async function addPlant() {
    if (!form.plantId) {
      setError("Выберите растение из справочника");
      return;
    }

    setError("");

    try {
      await api.userPlants.create({
        plantId: Number(form.plantId),
        note: form.note || null,
        nextWateringDate: form.nextWateringDate || null,
        nextRepottingDate: form.nextRepottingDate || null,
      });
    } catch (requestError) {
      setError(
        (requestError.message || "").includes("already")
          ? "Это растение уже есть в вашем списке"
          : "Не удалось добавить растение",
      );
      return;
    }

    setForm({
      plantId: "",
      note: "",
      nextWateringDate: "",
      nextRepottingDate: "",
    });
    loadUserPlants();
  }

  return (
    <section>
      <div className="page-title">
        <h1>Мои растения</h1>
        <p>Личный список растений пользователя.</p>
      </div>

      <div className="form-panel">
        <select
          className="input"
          value={form.plantId}
          onChange={(event) => updateField("plantId", event.target.value)}
        >
          <option value="">Выберите растение</option>
          {catalog.map((plant) => (
            <option key={plant.id} value={plant.id}>
              {plant.name}
            </option>
          ))}
        </select>

        <textarea
          className="input textarea"
          value={form.note}
          onChange={(event) => updateField("note", event.target.value)}
          placeholder="Заметка"
        />

        <div className="form-row">
          <label>
            Следующий полив
            <input
              className="input"
              type="date"
              value={form.nextWateringDate}
              onChange={(event) => updateField("nextWateringDate", event.target.value)}
            />
          </label>

          <label>
            Следующая пересадка
            <input
              className="input"
              type="date"
              value={form.nextRepottingDate}
              onChange={(event) => updateField("nextRepottingDate", event.target.value)}
            />
          </label>
        </div>

        <button className="button" onClick={addPlant}>
          Добавить
        </button>
      </div>

      {error && <p className="error">{error}</p>}

      <div className="list">
        {plants.map((plant) => (
          <div className="list-item" key={plant.id}>
            <div>
              <h2>{plant.plantName || plant.nickname}</h2>
              {plant.addedAt && (
                <p className="muted">Добавлено: {new Date(plant.addedAt).toLocaleDateString()}</p>
              )}
              {plant.note && <p className="muted">{plant.note}</p>}
              {plant.nextWateringDate && (
                <p className="muted">Полив: {new Date(plant.nextWateringDate).toLocaleDateString()}</p>
              )}
              {plant.nextRepottingDate && (
                <p className="muted">
                  Пересадка: {new Date(plant.nextRepottingDate).toLocaleDateString()}
                </p>
              )}
            </div>

            <button
              className="button button-danger"
              onClick={() => api.userPlants.remove(plant.id).then(loadUserPlants)}
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
