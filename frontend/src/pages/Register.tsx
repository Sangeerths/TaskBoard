import { useState, type ChangeEvent, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { ArrowRight, CheckCircle2, Columns3, Eye, EyeOff } from "lucide-react";
import { api } from "../services/api";
import "./Register.css";

interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  dateOfBirth: string;
  gender: string;
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
}

interface RegisterResponse {
  statusCode: number;
  message: string;
}

const initialForm: RegisterRequest = {
  email: "",
  password: "",
  firstName: "",
  lastName: "",
  phoneNumber: "",
  dateOfBirth: "",
  gender: "",
  address: "",
  city: "",
  state: "",
  country: "",
  postalCode: "",
};

export default function Register() {
  const navigate = useNavigate();
  const [form, setForm] = useState<RegisterRequest>(initialForm);
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  function handleChange(
    event: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>,
  ) {
    const { name, value } = event.target;
    setForm((previous) => ({ ...previous, [name]: value }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setSuccess("");

    if (form.password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }
    if (form.password.length < 6) {
      setError("Password must contain at least 6 characters.");
      return;
    }
    if (!form.dateOfBirth) {
      setError("Please select your date of birth.");
      return;
    }

    const birthDate = new Date(`${form.dateOfBirth}T00:00:00.000Z`);
    if (Number.isNaN(birthDate.getTime()) || birthDate > new Date()) {
      setError("Please enter a valid date of birth.");
      return;
    }

    setLoading(true);
    try {
      const request: RegisterRequest = {
        ...form,
        email: form.email.trim(),
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        dateOfBirth: birthDate.toISOString(),
      };
      const response = await api.post<RegisterResponse>("api/Auth/register", request);
      setSuccess(response.message || "Registration successful!");
      window.setTimeout(() => navigate("/login"), 1200);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to register. Please try again.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="register-page">
      <section className="register-brand-panel" aria-label="About TaskBoard">
        <Link to="/" className="register-brand">
          <span className="register-brand-icon"><Columns3 size={22} /></span>
          <span>TaskBoard</span>
        </Link>
        <div className="register-brand-copy">
          <span className="register-kicker">A CLEARER WAY TO WORK</span>
          <h1>Your next big thing starts here.</h1>
          <p>Create your workspace and give every project a clear place to move forward.</p>
          <ul>
            <li><CheckCircle2 size={18} /> Organize work in one place</li>
            <li><CheckCircle2 size={18} /> See progress as it happens</li>
            <li><CheckCircle2 size={18} /> Keep your team in sync</li>
          </ul>
        </div>
        <p className="register-panel-footer">Plan together. Make progress.</p>
        <div className="register-panel-orb register-panel-orb--one" />
        <div className="register-panel-orb register-panel-orb--two" />
      </section>

      <section className="register-content">
        <div className="register-card">
          <p className="register-card-kicker">GET STARTED</p>
          <h2>Create your account</h2>
          <p className="register-intro">A few details now, then you can start organizing your projects.</p>

          {error && <div className="form-error" role="alert">{error}</div>}
          {success && <div className="form-success" role="status">{success}</div>}

          <form onSubmit={handleSubmit}>
            <section className="register-section">
              <div className="register-section-heading">
                <h3>Account details</h3>
                <p>Your name and sign-in information.</p>
              </div>
              <div className="form-grid">
                <label>
                  First name
                  <input name="firstName" value={form.firstName} onChange={handleChange} required maxLength={100} autoComplete="given-name" />
                </label>
                <label>
                  Last name
                  <input name="lastName" value={form.lastName} onChange={handleChange} required maxLength={100} autoComplete="family-name" />
                </label>
                <label className="full-width">
                  Email address
                  <input name="email" type="email" value={form.email} onChange={handleChange} required maxLength={254} autoComplete="email" />
                </label>
                <label>
                  Password
                  <span className="register-password-field">
                    <input name="password" type={showPassword ? "text" : "password"} value={form.password} onChange={handleChange} required minLength={6} autoComplete="new-password" />
                    <button type="button" className="register-password-toggle" aria-label={showPassword ? "Hide password" : "Show password"} aria-pressed={showPassword} onClick={() => setShowPassword((visible) => !visible)}>
                      {showPassword ? <EyeOff size={17} /> : <Eye size={17} />}
                    </button>
                  </span>
                  <small>Use at least 6 characters.</small>
                </label>
                <label>
                  Confirm password
                  <span className="register-password-field">
                    <input type={showConfirmPassword ? "text" : "password"} value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} required autoComplete="new-password" />
                    <button type="button" className="register-password-toggle" aria-label={showConfirmPassword ? "Hide password" : "Show password"} aria-pressed={showConfirmPassword} onClick={() => setShowConfirmPassword((visible) => !visible)}>
                      {showConfirmPassword ? <EyeOff size={17} /> : <Eye size={17} />}
                    </button>
                  </span>
                </label>
              </div>
            </section>

            <section className="register-section">
              <div className="register-section-heading">
                <h3>About you</h3>
                <p>Contact details for your profile.</p>
              </div>
              <div className="form-grid">
                <label>
                  Phone number
                  <input name="phoneNumber" type="tel" value={form.phoneNumber} onChange={handleChange} required maxLength={30} autoComplete="tel" />
                </label>
                <label>
                  Date of birth
                  <input name="dateOfBirth" type="date" value={form.dateOfBirth} onChange={handleChange} required max={new Date().toISOString().slice(0, 10)} autoComplete="bday" />
                </label>
                <label>
                  Gender
                  <select name="gender" value={form.gender} onChange={handleChange} required>
                    <option value="">Select gender</option>
                    <option value="Male">Male</option>
                    <option value="Female">Female</option>
                    <option value="Other">Other</option>
                    <option value="Prefer not to say">Prefer not to say</option>
                  </select>
                </label>
              </div>
            </section>

            <section className="register-section">
              <div className="register-section-heading">
                <h3>Location</h3>
                <p>Your address information.</p>
              </div>
              <div className="form-grid">
                <label className="full-width">
                  Street address
                  <textarea name="address" value={form.address} onChange={handleChange} required rows={2} maxLength={300} autoComplete="street-address" />
                </label>
                <label>
                  City
                  <input name="city" value={form.city} onChange={handleChange} required maxLength={100} autoComplete="address-level2" />
                </label>
                <label>
                  State or region
                  <input name="state" value={form.state} onChange={handleChange} required maxLength={100} autoComplete="address-level1" />
                </label>
                <label>
                  Postal code
                  <input name="postalCode" value={form.postalCode} onChange={handleChange} required maxLength={20} autoComplete="postal-code" />
                </label>
                <label>
                  Country
                  <input name="country" value={form.country} onChange={handleChange} required maxLength={100} autoComplete="country-name" />
                </label>
              </div>
            </section>

            <button className="register-submit" type="submit" disabled={loading}>
              {loading ? "Creating your account…" : "Create account"}
              {!loading && <ArrowRight size={17} />}
            </button>
          </form>

          <p className="login-link">Already have an account? <Link to="/login">Sign in</Link></p>
        </div>
      </section>
    </main>
  );
}
