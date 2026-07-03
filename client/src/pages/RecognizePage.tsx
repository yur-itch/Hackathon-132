// Набросок: распознавание растения по фото (Pl@ntNet). Без ключа работает на фикстурах.
import { useState } from "react";
import { api } from "../api/client";
import type { RecognitionResult } from "../api/types";

const mockScenarios = [
  { value: "", label: "— реальный ключ / выкл —" },
  { value: "monstera", label: "monstera (Matched)" },
  { value: "secondmatch", label: "secondmatch (Matched)" },
  { value: "unknown", label: "unknown (нет карточки)" },
  { value: "lowconfidence", label: "lowconfidence" },
];

export default function RecognizePage() {
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [scenario, setScenario] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<RecognitionResult | null>(null);

  function onFileChange(f: File | null) {
    setFile(f);
    setResult(null);
    setError(null);
    setPreview(f ? URL.createObjectURL(f) : null);
  }

  async function onSubmit() {
    if (!file) return;
    setLoading(true);
    setError(null);
    setResult(null);
    try {
      const r = await api.recognition.identify(file, "auto", scenario || undefined);
      setResult(r);
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="max-w-lg">
      <h2 className="mb-4 text-lg font-semibold">📷 Распознать растение</h2>

      <input
        type="file"
        accept="image/jpeg,image/png"
        onChange={(e) => onFileChange(e.target.files?.[0] ?? null)}
        className="mb-3 block w-full text-sm"
      />

      {preview && (
        <img src={preview} alt="preview" className="mb-3 h-48 w-full rounded-md object-cover" />
      )}

      <label className="mb-1 block text-sm text-gray-600">Мок-сценарий (без ключа API)</label>
      <select
        value={scenario}
        onChange={(e) => setScenario(e.target.value)}
        className="mb-3 w-full rounded-md border px-3 py-2"
      >
        {mockScenarios.map((s) => (
          <option key={s.value} value={s.value}>
            {s.label}
          </option>
        ))}
      </select>

      <button
        onClick={onSubmit}
        disabled={!file || loading}
        className="rounded-md bg-green-600 px-4 py-2 text-white disabled:opacity-50"
      >
        {loading ? "Распознаём…" : "Распознать"}
      </button>

      {error && <p className="mt-4 text-sm text-red-600">Ошибка: {error}</p>}

      {result && (
        <div className="mt-4 rounded-lg border bg-white p-4">
          <p className="mb-2 text-xs uppercase tracking-wide text-gray-500">{result.status}</p>
          <p className="mb-2">{result.message}</p>

          {result.matchedCard && (
            <div className="mb-2 rounded-md bg-green-50 p-2">
              <p className="font-semibold">{result.matchedCard.name}</p>
              <p className="text-sm italic text-gray-500">{result.matchedCard.latinName}</p>
            </div>
          )}

          {!result.matchedCard && result.recognizedLatinName && (
            <p className="text-sm italic text-gray-500">{result.recognizedLatinName}</p>
          )}

          {result.topScore != null && (
            <p className="text-sm text-gray-500">
              Уверенность: {(result.topScore * 100).toFixed(0)}%
            </p>
          )}

          {result.candidates.length > 0 && (
            <details className="mt-2 text-sm">
              <summary className="cursor-pointer text-gray-500">
                Кандидаты ({result.candidates.length})
              </summary>
              <ul className="mt-1 list-disc pl-5">
                {result.candidates.map((c, i) => (
                  <li key={i}>
                    {c.latinName} — {(c.score * 100).toFixed(0)}%
                  </li>
                ))}
              </ul>
            </details>
          )}
        </div>
      )}
    </div>
  );
}
