
import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../context/authContextStore";
import { api } from "../services/api";
import {
  ArrowRight,
  Check,
  FolderKanban,
  LayoutDashboard,
  LogOut,
  Pencil,
  Plus,
  X,
} from "lucide-react";
import "./ProjectsPage.css";
import { Link, useNavigate } from "react-router-dom";

interface Project {
  projectId?: string;
  id?: string;
  name: string;
  description?: string | null;
  createdAt?: string;
  updatedAt?: string;
}

interface ProjectRequest {
  name: string;
  description: string;
}

interface ApiMessage {
  message?: string;
  statusCode?: number;
}

function getProjects(data: unknown): Project[] {
  if (Array.isArray(data)) {
    return data as Project[];
  }

  if (typeof data === "object" && data !== null) {
    const result = data as Record<string, unknown>;

    for (const key of ["projects", "items", "data", "results"]) {
      if (Array.isArray(result[key])) {
        return result[key] as Project[];
      }
    }
  }

  return [];
}

function getProjectId(project: Project): string {
  return project.projectId ?? project.id ?? "";
}

function formatProjectDate(value?: string): string {
  if (!value) return "Workspace project";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "Workspace project";
  return `Updated ${date.toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  })}`;
}

function getUserName(user: ReturnType<typeof useAuth>["user"]): string {
  if (!user) return "Signed in";
  if (typeof user.name === "string" && user.name.trim()) return user.name.trim();

  const firstName =
    typeof user.firstName === "string" ? user.firstName.trim() : "";
  const lastName =
    typeof user.lastName === "string" ? user.lastName.trim() : "";
  const fullName = [firstName, lastName].filter(Boolean).join(" ");

  return fullName || user.email || "Profile";
}

function ProjectsPage() {
  const { user, logout } = useAuth();

  const [projects, setProjects] = useState<Project[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingProjectId, setEditingProjectId] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);
  const [notice, setNotice] = useState("");
  const [projectName, setProjectName] = useState("");
  const [projectDescription, setProjectDescription] = useState("");
    const navigate = useNavigate();
  const loadProjects = useCallback(async (signal?: AbortSignal) => {
    if (signal?.aborted) return;
    setLoading(true);
    setError("");

    try {
      const data = await api.get<unknown>("api/Projects", { signal });
      if (signal?.aborted) return;
      setProjects(getProjects(data));
    } catch (err) {
      if (signal?.aborted) return;
      setProjects([]);
      setError(
        err instanceof Error ? err.message : "Unable to load projects.",
      );
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    void Promise.resolve().then(() => loadProjects(controller.signal));
    return () => controller.abort();
  }, [loadProjects]);

  function startNewProject() {
    setEditingProjectId(null);
    setProjectName("");
    setProjectDescription("");
    setError("");
    setNotice("");
    setShowCreateForm(true);
  }

  function startEditingProject(project: Project) {
    const projectId = getProjectId(project);
    if (!projectId) return;
    setEditingProjectId(projectId);
    setProjectName(project.name ?? "");
    setProjectDescription(project.description ?? "");
    setError("");
    setNotice("");
    setShowCreateForm(true);
  }

  async function handleSaveProject(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    setError("");
    setNotice("");

    const name = projectName.trim();
    const description = projectDescription.trim();

    if (!name) {
      setError("Please enter a project name.");
      return;
    }

    setCreating(true);

    try {
      const request: ProjectRequest = { name, description };

      const response = editingProjectId
        ? await api.put<ApiMessage>(
            `api/Projects/${encodeURIComponent(editingProjectId)}`,
            request,
          )
        : await api.post<ApiMessage>("api/Projects", request);

      setProjectName("");
      setProjectDescription("");
      setShowCreateForm(false);
      setEditingProjectId(null);
      setNotice(
        response?.message ||
          (editingProjectId ? "Project updated successfully." : "Project created successfully."),
      );

      await loadProjects();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : editingProjectId
            ? "Unable to update project."
            : "Unable to create project.",
      );
    } finally {
      setCreating(false);
    }
  }

  return (
    <div className="projects-layout">
      <aside className="projects-sidebar">
        <Link className="projects-brand" to="/">
          <span className="projects-brand-icon">
            <LayoutDashboard size={22} />
          </span>
          <span>TaskBoard</span>
        </Link>

        <div className="sidebar-label">WORKSPACE</div>

        <nav className="sidebar-nav" aria-label="Main navigation">
          <Link className="sidebar-link active" to="/projects">
            <FolderKanban size={19} />
            <span>Projects</span>
          </Link>
        </nav>

        <div className="sidebar-bottom">
          <div className="sidebar-user">
            <div className="user-avatar">
              {getUserName(user)[0].toUpperCase()}
            </div>
            <div className="sidebar-user-info">
              <strong>{getUserName(user)}</strong>
              <span title={user?.email ?? ""}>
                {user?.email ?? "Signed in"}
              </span>
            </div>
          </div>

          <button
            className="logout-button"
            onClick={() => {
              logout();
              navigate("/");
            }}
          >
            <LogOut size={18} />
            <span>Log out</span>
          </button>
        </div>
      </aside>

      <main className="projects-main">
        <header className="projects-topbar">
          <div className="breadcrumb">
            Workspace <span>/</span> <strong>Projects</strong>
          </div>
          <div className="topbar-account">
            <span className="online-indicator" />
            <span>Workspace</span>
          </div>
        </header>

        <section className="projects-content">
          <div className="projects-heading">
            <div>
              <p className="eyebrow">YOUR WORKSPACE</p>
              <h1>Projects</h1>
              <p className="projects-subtitle">
                Keep your work organized and move your ideas forward.
              </p>
            </div>

            <button
              className="create-project-button"
              onClick={startNewProject}
            >
              <Plus size={18} />
              New project
            </button>
          </div>

          {error && (
            <div className="projects-alert error-alert" role="alert">
              <span>{error}</span>
              <button onClick={() => void loadProjects()}>Try again</button>
            </div>
          )}

          {notice && (
            <div className="projects-alert success-alert" role="status">
              <Check size={18} />
              <span>{notice}</span>
              <button
                aria-label="Dismiss message"
                onClick={() => setNotice("")}
              >
                <X size={17} />
              </button>
            </div>
          )}

          {showCreateForm && (
            <section className="create-project-panel">
              <div className="create-panel-heading">
                <div>
                  <h2>{editingProjectId ? "Edit project" : "Create a project"}</h2>
                  <p>{editingProjectId ? "Update the project name or description." : "Give your next idea a place to grow."}</p>
                </div>
              </div>

              <form onSubmit={handleSaveProject}>
                <label>
                  Project name
                  <input
                    value={projectName}
                    onChange={(event) => setProjectName(event.target.value)}
                    placeholder="e.g. Website redesign"
                    maxLength={150}
                    required
                    autoFocus
                  />
                </label>

                <label>
                  Description
                  <textarea
                    value={projectDescription}
                    onChange={(event) =>
                      setProjectDescription(event.target.value)
                    }
                    placeholder="What is this project about?"
                    rows={3}
                  />
                </label>

                <div className="create-form-actions">
                  <button
                    type="button"
                    className="cancel-button"
                    onClick={() => {
                      setShowCreateForm(false);
                      setEditingProjectId(null);
                    }}
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="create-project-button"
                    disabled={creating}
                  >
                    {creating
                      ? editingProjectId ? "Saving..." : "Creating..."
                      : editingProjectId ? "Save changes" : "Create project"}
                  </button>
                </div>
              </form>
            </section>
          )}

          <div className="project-list-heading">
            <div>
              <h2>All projects</h2>
              <p>Your projects in one place.</p>
            </div>
            {!loading && projects.length > 0 && (
              <span className="projects-count">
                {projects.length} {projects.length === 1 ? "project" : "projects"}
              </span>
            )}
          </div>

          {loading ? (
            <div className="projects-state">
              <div className="loading-spinner" />
              <p>Loading your projects...</p>
            </div>
          ) : projects.length > 0 ? (
            <div className="project-grid">
              {projects.map((project, index) => (
                <article
                  className="project-card"
                  key={getProjectId(project) || `${project.name}-${index}`}
                >
                  <div className="project-card-main">
                    <div className="project-card-topline">
                      <span className="project-card-icon"><FolderKanban size={19} /></span>
                      <span className="project-card-label">PROJECT</span>
                      {getProjectId(project) && (
                        <button
                          type="button"
                          className="project-edit-button"
                          aria-label={`Edit ${project.name || "project"}`}
                          title="Edit project"
                          onClick={() => startEditingProject(project)}
                        >
                          <Pencil size={16} />
                        </button>
                      )}
                    </div>
                    <button
                      type="button"
                      className="project-name-button"
                      disabled={!getProjectId(project)}
                      title={
                        getProjectId(project)
                          ? `Open ${project.name || "project"}`
                          : "This project has no ID and cannot be opened"
                      }
                      onClick={() =>
                        navigate(
                          `/projects/${encodeURIComponent(getProjectId(project))}`,
                        )
                      }
                    >
                      {project.name || "Untitled project"}
                    </button>
                    {project.description && (
                      <p className="project-card-description">{project.description}</p>
                    )}
                    <div className="project-card-footer">
                      <span>{formatProjectDate(project.updatedAt ?? project.createdAt)}</span>
                      <span className="project-open-label">Open project <ArrowRight size={15} /></span>
                    </div>
                  </div>
                </article>
              ))}
            </div>
          ) : (
            <div className="projects-empty">
              <div className="empty-icon">
                <FolderKanban size={28} />
              </div>
              <h3>No projects yet</h3>
              <p>
                Create your first project to start organizing your work.
              </p>
              <button
                className="create-project-button"
                onClick={startNewProject}
              >
                <Plus size={18} /> Create your first project
              </button>
            </div>
          )}
        </section>
      </main>
    </div>
  );
}

export default ProjectsPage;
