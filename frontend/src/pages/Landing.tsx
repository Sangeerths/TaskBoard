
import { Link } from "react-router";
import {
  ArrowRight,
  CheckCircle2,
  Columns3,
  FolderKanban,
  Users,
  Zap,
}
 from "lucide-react";
import { useAuth } from "../context/authContextStore";
import "./Landing.css";

const features = [
  {
    icon: FolderKanban,
    title: "Organize projects",
    description:
      "Keep your projects, plans, and priorities organized in one place.",
  },
  {
    icon: Columns3,
    title: "Visual Kanban boards",
    description:
      "Track tasks from start to finish with a clear, visual workflow.",
  },
  {
    icon: Users,
    title: "Work together",
    description:
      "Bring your team together to coordinate work and stay on the same page.",
  },
];

function Landing() {
  const { user, loading } = useAuth();

  return (
    <main className="landing-page">
      <nav className="landing-nav">
        <div className="brand">
          <span className="brand-icon">
            <Columns3 size={23} />
          </span>
          <span>TaskBoard</span>
        </div>

        {!loading && (
          <div className="nav-actions">
            {user ? (
              <Link to="/projects" className="nav-register">
              {user.name ||
                [user.firstName, user.lastName]
                  .filter((part): part is string => typeof part === "string" && part.trim().length > 0)
                  .join(" ") ||
                user.email ||
                "Profile"}{" "}
              <ArrowRight size={16} />
            </Link>
            ) : (
              <>
                <Link to="/login" className="nav-login">
                  Login
                </Link>
                <Link to="/register" className="nav-register">
                  Get Started <ArrowRight size={16} />
                </Link>
              </>
            )}
          </div>
        )}
      </nav>

      <section className="hero">
        <div className="hero-content">
          <span className="hero-label">
            <Zap size={15} /> YOUR WORK, ORGANIZED
          </span>

          <h1>
            Plan better.
            <br />
            Work <span>smarter.</span>
            <br />
            Finish together.
          </h1>

          <p className="hero-description">
            TaskBoard helps you turn ideas into action. Organize projects,
            manage tasks, and see your progress clearly — all in one place.
          </p>

          {!loading && (
            <div className="hero-actions">
              {user ? (
                <Link to="/projects" className="primary-button">
                  Open your workspace <ArrowRight size={18} />
                </Link>
              ) : (
                <>
                  <Link to="/register" className="primary-button">
                    Get started for free <ArrowRight size={18} />
                  </Link>
                  <Link to="/login" className="secondary-button">
                    I already have an account
                  </Link>
                </>
              )}
            </div>
          )}

          <div className="hero-points">
            <span>
              <CheckCircle2 size={16} /> Organized workflows
            </span>
            <span>
              <CheckCircle2 size={16} /> Clear project progress
            </span>
          </div>

          <div className="hero-stats" aria-label="TaskBoard highlights">
            <div><strong>One</strong><span>shared workspace</span></div>
            <div><strong>Clear</strong><span>next steps</span></div>
            <div><strong>Every</strong><span>project in view</span></div>
          </div>
        </div>

        <div className="board-preview" aria-label="Example task board preview">
          <div className="preview-topbar">
            <div>
              <span className="preview-eyebrow">YOUR WORKSPACE</span>
              <h2>Website Redesign</h2>
            </div>
            <span className="preview-live"><span /> Project board</span>
          </div>

          <div className="preview-progress">
            <div><span>Project progress</span><strong>68%</strong></div>
            <span className="preview-progress-track"><span /></span>
          </div>

          <div className="preview-columns">
            <div className="preview-column">
              <div className="column-heading">
                <span className="status-dot todo" />
                Started <span className="task-count">2</span>
              </div>
              <div className="task-card">
                <span className="task-tag purple">Planning</span>
                <strong>Define project goals</strong>
                <small>Set the scope and priorities</small>
                <div className="task-footer">
                  <span>☷ 3</span>
                  <span className="avatar avatar-one">A</span>
                </div>
              </div>
              <div className="task-card">
                <span className="task-tag blue">Research</span>
                <strong>Review references</strong>
                <div className="task-footer">
                  <span>☷ 2</span>
                  <span className="avatar avatar-two">J</span>
                </div>
              </div>
            </div>

            <div className="preview-column">
              <div className="column-heading">
                <span className="status-dot progress" />
                In progress <span className="task-count">1</span>
              </div>
              <div className="task-card">
                <span className="task-tag orange">Design</span>
                <strong>Create wireframes</strong>
                <small>Build the first layout draft</small>
                <div className="task-footer">
                  <span>☷ 4</span>
                  <span className="avatar avatar-three">M</span>
                </div>
              </div>
            </div>

            <div className="preview-column">
              <div className="column-heading">
                <span className="status-dot done" />
                Completed <span className="task-count">1</span>
              </div>
              <div className="task-card">
                <span className="task-tag green">Setup</span>
                <strong>Create workspace</strong>
                <small>Project is ready to go</small>
                <div className="task-footer">
                  <span>☷ 1</span>
                  <span className="avatar avatar-four">S</span>
                </div>
              </div>
            </div>
          </div>

          <div className="preview-note">
            <CheckCircle2 size={17} />
            <span>Small steps. Meaningful progress.</span>
          </div>
        </div>
      </section>

      <section className="features-section">
        <div className="section-heading">
          <span>MADE FOR GETTING THINGS DONE</span>
          <h2>Everything in its right place.</h2>
          <p>
            Spend less time figuring out what to do and more time making
            progress.
          </p>
        </div>

        <div className="feature-grid">
          {features.map((feature) => {
            const Icon = feature.icon;

            return (
              <article className="feature-card" key={feature.title}>
                <div className="feature-icon">
                  <Icon size={23} />
                </div>
                <h3>{feature.title}</h3>
                <p>{feature.description}</p>
              </article>
            );
          })}
        </div>
      </section>

      <section className="bottom-cta">
        <div>
          <h2>Ready to get organized?</h2>
          <p>Your next productive day starts with a plan.</p>
        </div>
        <Link
          to={user ? "/projects" : "/register"}
          className="cta-button"
        >
          {user ? "Open your workspace" : "Create your account"}{" "}
          <ArrowRight size={18} />
        </Link>
      </section>

      <footer className="landing-footer">
        <Link to="/" className="brand footer-brand">
          <span className="brand-icon">
            <Columns3 size={20} />
          </span>
          TaskBoard
        </Link>
        <span>Plan together. Make progress.</span>
      </footer>
    </main>
  );
}

export default Landing;
