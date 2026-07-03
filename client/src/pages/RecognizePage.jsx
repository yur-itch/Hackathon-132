import { useState } from "react";
import { api } from "../api/client.js";

const mockScenarios = [
  { value: "", label: "Реальный API или настройка backend" },
  { value: "monstera", label: "monstera: найдена карточка" },
  { value: "secondmatch", label: "secondmatch: найден второй кандидат" },
  { value: "unknown", label: "unknown: карточки нет" },
  { value: "lowconfidence", label: "lowconfidence: низкая уверенность" },
];

export default function RecognizePage() {
  const [file, setFile] = useState(null);
  const [preview, setPreview] = useState("");
  const [scenario, setScenario] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [result, setResult] = useState(null);

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

    try {
      const response = await api.recognition.identify(file, "auto", scenario);
      setResult(response);
    } catch (requestError) {
      setError(requestError.message || "Не удалось распознать растение");
    } finally {
      setLoading(false);
    }
  }

  return (
    <section>
      <div className="page-title">
        <h1>Распознать растение</h1>
        <p>Загрузите фото, backend отправит его в сервис распознавания или мок-сценарий.</p>
      </div>

      <div className="form-panel narrow">
        <input
          className="input"
          type="file"
          accept="image/jpeg,image/png"
          onChange={(event) => handleFileChange(event.target.files[0] || null)}
        />

        {preview && <img className="preview-image" src={preview} alt="Предпросмотр" />}

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

        <button className="button" onClick={submitPhoto} disabled={!file || loading}>
          {loading ? "Распознаем..." : "Распознать"}
        </button>
      </div>

      {error && <p className="error">{error}</p>}

      {result && (
        <div className="result-box">
          <p className="result-status">{result.status}</p>
          <p>{result.message}</p>

          {result.matchedCard && (
            <div className="result-card">
              <h2>{result.matchedCard.name}</h2>
              {result.matchedCard.latinName && (
                <p className="latin-name">{result.matchedCard.latinName}</p>
              )}
            </div>
          )}

          {!result.matchedCard && result.recognizedLatinName && (
            <p className="latin-name">{result.recognizedLatinName}</p>
          )}

          {result.topScore !== null && result.topScore !== undefined && (
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
