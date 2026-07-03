import { createContext, useContext, useEffect, useState } from "react";
import { api, setGuestMode } from "../api/client.js";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  function refresh() {
    return api.auth
      .me()
      .then((me) => {
        setGuestMode(false);
        setUser(me);
      })
      .catch(() => {
        setGuestMode(true);
        setUser(null);
      })
      .finally(() => setLoading(false));
  }

  useEffect(refresh, []);

  // После входа гостевые данные из localStorage переезжают в аккаунт,
  // прежде чем страницы перечитают коллекцию с бэка.
  async function login(email, password) {
    const loggedInUser = await api.auth.login(email, password);
    setGuestMode(false);
    await api.guest.importToAccount().catch(() => {});
    setUser(loggedInUser);
  }

  async function register(email, password, displayName) {
    const registeredUser = await api.auth.register(email, password, displayName);
    setGuestMode(false);
    await api.guest.importToAccount().catch(() => {});
    setUser(registeredUser);
  }

  async function logout() {
    await api.auth.logout();
    setGuestMode(true);
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
