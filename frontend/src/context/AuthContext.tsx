import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import { authService } from "../services/authService";
import type { CurrentUser } from "../services/authService";
import { setUnauthorizedHandler, tokenStorage } from "../services/api";
import { AuthContext } from "./authContextStore";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [loading, setLoading] = useState(true);

  // Any 401 from the API logs the user out
  useEffect(() => {
    const handleUnauthorized = () => setUser(null);
    setUnauthorizedHandler(handleUnauthorized);
    return () => setUnauthorizedHandler(() => undefined);
  }, []);

  // Restore session on page refresh
  useEffect(() => {
    async function restore() {
      if (!tokenStorage.get()) {
        setLoading(false);
        return;
      }
      try {
        setUser(await authService.me());
      } catch {
        tokenStorage.clear();
        setUser(null);
      } finally {
        setLoading(false);
      }
    }
    void restore();
  }, []);

  async function login(email: string, password: string) {
    tokenStorage.clear(); // never send a stale token to the login endpoint
    const data = await authService.login({ email, password });
    const token = data.accessToken ?? data.token;
    if (!token) throw new Error("Login succeeded but no token was returned.");

    tokenStorage.set(token);
    try {
      setUser(await authService.me());
    } catch (error) {
      tokenStorage.clear();
      setUser(null);
      throw error;
    }
  }

  function logout() {
    tokenStorage.clear();
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
