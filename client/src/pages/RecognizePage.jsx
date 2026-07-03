import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import PlantCard from "../components/PlantCard.jsx";

const recognitionMocksEnabled = import.meta.env.VITE_ENABLE_RECOGNITION_MOCKS === "true";

const mockScenarios = [
  { value: "", label: "Real API" },
  { value: "monstera", label: "monstera: matched plant card" },
  { value: "secondmatch", label: "secondmatch: matched second candidate" },
  { value: "unknown", label: "unknown: no card found" },
  { value: "lowconfidence", label: "lowconfidence: low confidence" },
];

export default function RecognizePage() {
  const [file, setFile] = useState(null);
  const [preview, setPreview] = useState("");
  const [scenario, setScenario] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [result, setResult] = useState(null);
  const [favoriteIds, setFavoriteIds] = useState([]);
  const [addStatus, setAddStatus] = useState("idle"); // idle | saving | added
  const [addError, setAddError] = useState("");

  useEffect(() => {
    api.favorites.ids().then(setFavoriteIds).catch(() => {});
  }, []);

  function handleFileChange(selectedFile) {
    setFile(selectedFile);
    setResult(null);
    setError("");
    setPreview(selectedFile ? URL.createObjectURL(selectedFile) : "");
  }

  async function submitPhoto() {
    if (!file) {
      return;
    }

    setLoading(true);
    setError("");
    setResult(null);
    setAddStatus("idle");
    setAddError("");

    try {
      const activeScenario = recognitionMocksEnabled ? scenario : "";
      const response = await api.recognition.identify(file, "auto", activeScenario);
      setResult(response);
    } catch (requestError) {
      setError(requestError.message || "Не удалось распознать растение");
    } finally {
      setLoading(false);
    }
  }

  async function toggleFavorite(plantId) {
    const isFavorite = favoriteIds.includes(plantId);

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

  async function addToMyPlants(plantId) {
    setAddStatus("saving");
    setAddError("");

    try {
      await api.userPlants.create({ plantId });
      setAddStatus("added");
    } catch (requestError) {
      if ((requestError.message || "").includes("already added")) {
        setAddStatus("added");
      } else {
        setAddStatus("idle");
        setAddError("Не удалось добавить растение в список");
      }
    }
  }

  return (
    <section>
      <div className="page-title">
        <h1>Распознать растение</h1>
        <p>Загрузите фото, backend отправит его в сервис распознавания.</p>
      </div>

      <div className="form-panel narrow">
        <input
          className="input"
          type="file"
          accept="image/jpeg,image/png"
          onChange={(event) => handleFileChange(event.target.files[0] || null)}
        />

        {preview && <img className="preview-image" src={preview} alt="Предпросмотр" />}

        {recognitionMocksEnabled && (
          <select
            className="input"
            value={scenario}
            onChange={(event) => setScenario(event.target.value)}
          >
            {mockScenarios.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </select>
        )}

        <button className="button" onClick={submitPhoto} disabled={!file || loading}>
          {loading ? "Распознаем..." : "Распознать"}
        </button>
      </div>

      {error && <p className="error">{error}</p>}
      {addError && <p className="error">{addError}</p>}

      {result && (
        <div className="result-box">
          <p className="result-status">{result.status}</p>
          <p>{result.message}</p>

          {result.matchedCard && (
            <PlantCard
              plant={result.matchedCard}
              actions={
                <>
                  <button
                    className="button"
                    onClick={() => addToMyPlants(result.matchedCard.id)}
                    disabled={addStatus !== "idle"}
                  >
                    {addStatus === "added"
                      ? "В моём списке ✓"
                      : addStatus === "saving"
                        ? "Добавляем…"
                        : "В мой список"}
                  </button>
                  <button
                    className={
                      favoriteIds.includes(result.matchedCard.id)
                        ? "button button-secondary"
                        : "button"
                    }
                    onClick={() => toggleFavorite(result.matchedCard.id)}
                  >
                    {favoriteIds.includes(result.matchedCard.id) ? "В избранном" : "В избранное"}
                  </button>
                </>
              }
            />
          )}

          {!result.matchedCard && result.recognizedLatinName && (
            <p className="latin-name">{result.recognizedLatinName}</p>
          )}

          {result.topScore != null && (
            <p className="muted">Уверенность: {(result.topScore * 100).toFixed(0)}%</p>
          )}

          {result.candidates?.length > 0 && (
            <details>
              <summary>Кандидаты: {result.candidates.length}</summary>
              <ul>
                {result.candidates.map((candidate, index) => (
                  <li key={`${candidate.latinName}-${index}`}>
                    {candidate.latinName}: {(candidate.score * 100).toFixed(0)}%
                  </li>
                ))}
              </ul>
            </details>
          )}
        </div>
      )}
    </section>
  );
}
