import { NavLink, Navigate, Route, Routes } from "react-router-dom";
import CatalogPage from "./pages/CatalogPage.jsx";
import FavoritesPage from "./pages/FavoritesPage.jsx";
import MyPlantsPage from "./pages/MyPlantsPage.jsx";
import RecognizePage from "./pages/RecognizePage.jsx";
import RemindersPage from "./pages/RemindersPage.jsx";
import ExchangePage from "./pages/ExchangePage.jsx";
import AuthPage from "./pages/AuthPage.jsx";
import { AuthProvider, useAuth } from "./context/AuthContext.jsx";
import "./App.css";

const menuItems = [
  { to: "/catalog", label: "Справочник" },
  { to: "/my-plants", label: "Мои растения" },
  { to: "/reminders", label: "Напоминания" },
  { to: "/favorites", label: "Избранное" },
  { to: "/recognize", label: "Распознать" },
  { to: "/exchange", label: "Обмен" },
];

function getLinkClass({ isActive }) {
  return isActive ? "nav-link nav-link-active" : "nav-link";
}

function Brand() {
  return (
    <div>
      <p className="brand">PlantCare</p>
      <p className="subtitle">Помощник по уходу за растениями</p>
    </div>
  );
}

function AppShell() {
  const { user, loading, logout } = useAuth();

  if (loading) {
    return (
      <div className="app">
        <header className="header">
          <div className="header-inner">
            <Brand />
          </div>
        </header>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="app">
        <header className="header">
          <div className="header-inner">
            <Brand />
          </div>
        </header>

        <main className="main">
          <AuthPage />
        </main>
      </div>
    );
  }

  return (
    <div className="app">
      <header className="header">
        <div className="header-inner">
          <Brand />

          <nav className="nav">
            {menuItems.map((item) => (
              <NavLink key={item.to} to={item.to} className={getLinkClass}>
                {item.label}
              </NavLink>
            ))}
          </nav>

          <div className="account-controls">
            <span className="muted">{user.displayName}</span>
            <button className="button button-secondary" onClick={logout}>
              Выйти
            </button>
          </div>
        </div>
      </header>

      <main className="main">
        <Routes>
          <Route path="/" element={<Navigate to="/catalog" replace />} />
          <Route path="/catalog" element={<CatalogPage />} />
          <Route path="/my-plants" element={<MyPlantsPage />} />
          <Route path="/reminders" element={<RemindersPage />} />
          <Route path="/favorites" element={<FavoritesPage />} />
          <Route path="/recognize" element={<RecognizePage />} />
          <Route path="/exchange" element={<ExchangePage />} />
          <Route path="*" element={<Navigate to="/catalog" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppShell />
    </AuthProvider>
  );
}
