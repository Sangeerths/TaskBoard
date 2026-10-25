import { useState } from "react";
import type { FormEvent } from "react";
import { Link, Navigate, useLocation, useNavigate } from "react-router-dom";
import { ArrowRight, CheckCircle2, Columns3, Eye, EyeOff, LockKeyhole } from "lucide-react";
import { useAuth } from "../context/authContextStore";
import "./LoginPage.css";

function getSafeReturnPath(state: unknown): string {
  if (typeof state !== "object" || state === null || !("from" in state)) {
    return "/projects";
  }
  const from = (state as { from?: { pathname?: unknown } }).from;
  const pathname = from?.pathname;
  if (
    typeof pathname !== "string" ||
    !pathname.startsWith("/") ||
    pathname.startsWith("//") ||
    pathname.includes("\\")
  ) {
    return "/projects";
  }
  return pathname;
}

export default function LoginScreen() {
  const { user, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const from = getSafeReturnPath(location.state);

  if (user) return <Navigate to={from} replace />;

  async function handleSubmit(event: FormEvent) {
    event.preventDefault(); // stop the browser's page reload
    if (loading) return;

    setLoading(true);
    setError("");
    try {
      await login(email.trim(), password);
      navigate(from, { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-brand-panel" aria-label="About TaskBoard">
        <Link to="/" className="login-brand">
          <span className="login-brand-icon"><Columns3 size={22} /></span>
          <span>TaskBoard</span>
        </Link>

        <div className="login-brand-copy">
          <span className="login-kicker">A CLEARER WAY TO WORK</span>
          <h1>Make room for work that matters.</h1>
          <p>Bring projects, people, and next steps together in one focused workspace.</p>
          <ul>
            <li><CheckCircle2 size={18} /> See what needs attention</li>
            <li><CheckCircle2 size={18} /> Keep every project moving</li>
            <li><CheckCircle2 size={18} /> Make progress as a team</li>
          </ul>
        </div>

        <p className="login-panel-footer">Plan together. Make progress.</p>
        <div className="login-panel-orb login-panel-orb--one" />
        <div className="login-panel-orb login-panel-orb--two" />
      </section>

      <section className="login-content">
        <div className="login-card">
          <div className="login-card-icon"><LockKeyhole size={19} /></div>
          <p className="login-card-kicker">WELCOME BACK</p>
          <h2>Sign in to your workspace</h2>
          <p className="login-card-description">Enter your account details to continue.</p>

          {error && <div className="login-error" role="alert">{error}</div>}

          <form className="login-fields" onSubmit={handleSubmit}>
            <label htmlFor="login-email">Email address</label>
            <input
              id="login-email"
              name="email"
              type="email"
              placeholder="you@example.com"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              autoComplete="username"
              maxLength={254}
              required
              autoFocus
            />

            <label htmlFor="login-password">Password</label>
            <div className="login-password-field">
              <input
                id="login-password"
                name="password"
                type={showPassword ? "text" : "password"}
                placeholder="Enter your password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                autoComplete="current-password"
                required
              />
              <button
                className="login-password-toggle"
                type="button"
                aria-label={showPassword ? "Hide password" : "Show password"}
                aria-pressed={showPassword}
                onClick={() => setShowPassword((visible) => !visible)}
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>

            <button className="login-submit" type="submit" disabled={loading}>
              {loading ? "Signing in…" : "Sign in"}
              {!loading && <ArrowRight size={17} />}
            </button>
            {loading && <p className="login-loading-note">Connecting to your workspace…</p>}
          </form>

          <p className="login-signup">
            New to TaskBoard? <Link to="/register">Create an account</Link>
          </p>
        </div>

        <p className="login-content-footer">Your projects are waiting for you.</p>
      </section>
    </main>
  );
}
