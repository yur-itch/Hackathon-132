import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext.jsx";

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const MIN_PASSWORD_LENGTH = 8;

export default function AuthPage() {
  const { login, register } = useAuth();
  const navigate = useNavigate();

  const [mode, setMode] = useState("login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  function validate() {
    if (!EMAIL_RE.test(email.trim())) {
      return "Некорректный email.";
    }

    if (password.length < MIN_PASSWORD_LENGTH) {
      return `Пароль должен быть не короче ${MIN_PASSWORD_LENGTH} символов.`;
    }

    if (mode === "register" && displayName.trim().length < 2) {
      return "Имя должно быть не короче 2 символов.";
    }

    return "";
  }

  async function submit(event) {
    event.preventDefault();

    const validationError = validate();
    if (validationError) {
      setError(validationError);
      return;
    }

    setError("");
    setLoading(true);

    try {
      if (mode === "login") {
        await login(email.trim(), password);
      } else {
        await register(email.trim(), password, displayName.trim());
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

      <form className="form-panel narrow" onSubmit={submit}>
        {mode === "register" && (
          <input
            className="input"
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
            placeholder="Имя"
            minLength={2}
            maxLength={100}
            required
          />
        )}

        <input
          className="input"
          type="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          placeholder="Email"
          required
        />

        <input
          className="input"
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          placeholder="Пароль (минимум 8 символов)"
          minLength={MIN_PASSWORD_LENGTH}
          required
        />

        <button className="button" type="submit" disabled={loading}>
          {loading ? "Подождите…" : mode === "login" ? "Войти" : "Зарегистрироваться"}
        </button>

        <button
          className="button button-secondary"
          type="button"
          onClick={() => {
            setMode(mode === "login" ? "register" : "login");
            setError("");
          }}
        >
          {mode === "login" ? "Нет аккаунта? Зарегистрироваться" : "Уже есть аккаунт? Войти"}
        </button>
      </form>

      {error && <p className="error">{error}</p>}
    </section>
  );
}
