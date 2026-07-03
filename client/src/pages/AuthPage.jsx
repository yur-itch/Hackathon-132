import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";

export default function AuthPage() {
  const { login, register } = useAuth();
  const navigate = useNavigate();

  const [mode, setMode] = useState("login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function submit() {
    setError("");
    setLoading(true);

    try {
      if (mode === "login") {
        await login(email, password);
      } else {
        await register(email, password, displayName);
      }
      navigate("/catalog");
    } catch (requestError) {
      setError(requestError.message || "Не удалось выполнить действие");
    } finally {
      setLoading(false);
    }
  }

  return (
    <section>
      <div className="page-title">
        <h1>{mode === "login" ? "Вход" : "Регистрация"}</h1>
        <p>
          {mode === "login" ? "Войдите, чтобы синхронизировать коллекцию между устройствами." : "Создайте аккаунт для своей коллекции."}
        </p>
      </div>

      <div className="form-panel narrow">
        {mode === "register" && (
          <input
            className="input"
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
            placeholder="Имя"
          />
        )}

        <input
          className="input"
          type="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          placeholder="Email"
        />

        <input
          className="input"
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          placeholder="Пароль"
        />

        <button className="button" onClick={submit} disabled={loading || !email || !password}>
          {loading ? "Подождите…" : mode === "login" ? "Войти" : "Зарегистрироваться"}
        </button>

        <button
          className="button button-secondary"
          onClick={() => {
            setMode(mode === "login" ? "register" : "login");
            setError("");
          }}
        >
          {mode === "login" ? "Нет аккаунта? Зарегистрироваться" : "Уже есть аккаунт? Войти"}
        </button>
      </div>

      {error && <p className="error">{error}</p>}
    </section>
  );
}
