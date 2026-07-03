import { NavLink, Navigate, Route, Routes } from "react-router-dom";
import CatalogPage from "./pages/CatalogPage.jsx";
import FavoritesPage from "./pages/FavoritesPage.jsx";
import MyPlantsPage from "./pages/MyPlantsPage.jsx";
import RemindersPage from "./pages/RemindersPage.jsx";
import "./App.css";

const menuItems = [
  { to: "/catalog", label: "Справочник" },
  { to: "/my-plants", label: "Мои растения" },
  { to: "/reminders", label: "Напоминания" },
  { to: "/favorites", label: "Избранное" },
];

function getLinkClass({ isActive }) {
  return isActive ? "nav-link nav-link-active" : "nav-link";
}

export default function App() {
  return (
    <div className="app">
      <header className="header">
        <div className="header-inner">
          <div>
            <p className="brand">PlantCare</p>
            <p className="subtitle">Помощник по уходу за растениями</p>
          </div>

          <nav className="nav">
            {menuItems.map((item) => (
              <NavLink key={item.to} to={item.to} className={getLinkClass}>
                {item.label}
              </NavLink>
            ))}
          </nav>
        </div>
      </header>

      <main className="main">
        <Routes>
          <Route path="/" element={<Navigate to="/catalog" replace />} />
          <Route path="/catalog" element={<CatalogPage />} />
          <Route path="/my-plants" element={<MyPlantsPage />} />
          <Route path="/reminders" element={<RemindersPage />} />
          <Route path="/favorites" element={<FavoritesPage />} />
        </Routes>
      </main>
    </div>
  );
}
