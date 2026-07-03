import { NavLink, Route, Routes, Navigate } from "react-router-dom";
import CatalogPage from "./pages/CatalogPage";
import MyPlantsPage from "./pages/MyPlantsPage";
import RemindersPage from "./pages/RemindersPage";
import FavoritesPage from "./pages/FavoritesPage";
import RecognizePage from "./pages/RecognizePage";

const tabs = [
  { to: "/catalog", label: "🌿 Справочник" },
  { to: "/my", label: "🪴 Мои растения" },
  { to: "/reminders", label: "⏰ Напоминания" },
  { to: "/favorites", label: "⭐ Избранное" },
  { to: "/recognize", label: "📷 Распознать" },
];

export default function App() {
  return (
    <div className="min-h-screen bg-gray-50 text-gray-900">
      <header className="border-b bg-white">
        <div className="mx-auto flex max-w-5xl items-center gap-6 px-4 py-3">
          <span className="text-lg font-semibold">PlantCare</span>
          <nav className="flex gap-1">
            {tabs.map((t) => (
              <NavLink
                key={t.to}
                to={t.to}
                className={({ isActive }) =>
                  `rounded-md px-3 py-1.5 text-sm ${
                    isActive ? "bg-green-100 text-green-800" : "text-gray-600 hover:bg-gray-100"
                  }`
                }
              >
                {t.label}
              </NavLink>
            ))}
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-4 py-6">
        <Routes>
          <Route path="/" element={<Navigate to="/catalog" replace />} />
          <Route path="/catalog" element={<CatalogPage />} />
          <Route path="/my" element={<MyPlantsPage />} />
          <Route path="/reminders" element={<RemindersPage />} />
          <Route path="/favorites" element={<FavoritesPage />} />
          <Route path="/recognize" element={<RecognizePage />} />
        </Routes>
      </main>
    </div>
  );
}
