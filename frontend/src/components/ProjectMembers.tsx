import { useCallback, useEffect, useState } from "react";
import type { FormEvent } from "react";
import { createPortal } from "react-dom";
import { Trash2, UserPlus, Users, X } from "lucide-react";
import { api } from "../services/api";
import "./ProjectMembers.css";

export interface ProjectMember {
  userId: string;
  email: string;
  role: string;
}

interface ProjectMembersProps {
  projectId: string;
  projectName: string;
  onMembersChange: (members: ProjectMember[]) => void;
}

interface MemberRequest {
  email: string;
  role: string;
}

interface RoleRequest {
  role: string;
}

function extractMembers(response: unknown): ProjectMember[] {
  // Supports a direct array or common response wrappers.
  let value: unknown = response;

  if (value && typeof value === "object" && !Array.isArray(value)) {
    const result = value as Record<string, unknown>;
    value = result.members ?? result.data ?? result.results ?? result;
  }

  if (!Array.isArray(value)) {
    return [];
  }

  return value.map((item) => {
    const member = item as Record<string, unknown>;

    return {
      userId: String(member.userId ?? member.UserId ?? ""),
      email: String(member.email ?? member.Email ?? ""),
      role: normalizeRole(String(member.role ?? member.Role ?? "Member")),
    };
  });
}

function normalizeRole(role: string): string {
  const normalized = role.trim().toLowerCase();
  if (normalized === "admin") return "Admin";
  if (normalized === "member") return "Member";
  return role;
}

export default function ProjectMembers({
  projectId,
  projectName,
  onMembersChange,
}: ProjectMembersProps) {
  const [members, setMembers] = useState<ProjectMember[]>([]);
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const [updatingRoleUserId, setUpdatingRoleUserId] = useState<string | null>(
    null,
  );
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  const loadMembers = useCallback(async () => {
    setLoading(true);
    setError("");

    try {
      const response = await api.get<unknown>(
        `api/Projects/${encodeURIComponent(projectId)}/members`
      );

      const projectMembers = extractMembers(response);
      setMembers(projectMembers);
      onMembersChange(projectMembers);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to load project members."
      );
    } finally {
      setLoading(false);
    }
  }, [onMembersChange, projectId]);

  useEffect(() => {
    void Promise.resolve().then(() => loadMembers());
  }, [loadMembers]);

  async function addMember(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setMessage("");

    const normalizedEmail = email.trim();

    if (!normalizedEmail) {
      setError("Enter the member's email address.");
      return;
    }

    setSaving(true);

    try {
      const request: MemberRequest = {
        email: normalizedEmail,
        role: "Member",
      };

      await api.post(
        `api/Projects/${encodeURIComponent(projectId)}/members`,
        request
      );

      setEmail("");
      setMessage("Member added successfully.");
      await loadMembers();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to add member."
      );
    } finally {
      setSaving(false);
    }
  }

  async function updateRole(
    userId: string,
    newRole: string
  ) {
    setError("");
    setMessage("");
    setUpdatingRoleUserId(userId);

    const request: RoleRequest = { role: newRole };

    try {
      await api.put(
        `api/Projects/${encodeURIComponent(projectId)}/members/${encodeURIComponent(userId)}`,
        request
      );

      const updatedMembers = members.map((member) =>
        member.userId === userId ? { ...member, role: newRole } : member,
      );
      setMembers(updatedMembers);
      onMembersChange(updatedMembers);
      setMessage("Member role updated.");
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to update member role."
      );
    } finally {
      setUpdatingRoleUserId(null);
    }
  }

  async function removeMember(member: ProjectMember) {
    const confirmed = window.confirm(
      `Remove ${member.email} from this project?`
    );

    if (!confirmed) return;

    setError("");
    setMessage("");

    try {
      await api.delete(
        `api/Projects/${encodeURIComponent(projectId)}/members/${encodeURIComponent(member.userId)}`
      );

      setMessage("Member removed successfully.");
      await loadMembers();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to remove member."
      );
    }
  }

  return (
    <>
      <button
        type="button"
        className="project-members__trigger"
        onClick={() => setIsOpen(true)}
      >
        <UserPlus size={15} /> Add member
      </button>

      {isOpen &&
        createPortal(
        <div
          className="project-members__backdrop"
          role="presentation"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget && !saving) {
              setIsOpen(false);
              setError("");
              setMessage("");
            }
          }}
        >
          <section
            className="project-members__dialog"
            role="dialog"
            aria-modal="true"
            aria-labelledby="project-members-title"
          >
            <header className="project-members__heading">
              <div className="project-members__heading-copy">
                <span className="project-members__dialog-icon"><Users size={20} /></span>
                <div>
                  <span className="project-members__dialog-kicker">TEAM WORKSPACE</span>
                  <h2 id="project-members-title">Project members</h2>
                  <p>{projectName}</p>
                </div>
              </div>
              <button
                type="button"
                className="project-members__close"
                aria-label="Close project members dialog"
                onClick={() => {
                  setIsOpen(false);
                  setError("");
                  setMessage("");
                }}
              >
                <X size={18} />
              </button>
            </header>

            {error && (
              <p className="project-members__notice project-members__notice--error">
                {error}
              </p>
            )}

            {message && (
              <p className="project-members__notice project-members__notice--success">
                {message}
              </p>
            )}

            <form className="project-members__form" onSubmit={addMember}>
              <div className="project-members__field">
                <label htmlFor={`member-email-${projectId}`}>
                  Email address
                </label>
                <input
                  id={`member-email-${projectId}`}
                  type="email"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  placeholder="user@example.com"
                  required
                  autoFocus
                />
              </div>

              <button type="submit" disabled={saving}>
                {saving ? "Adding..." : "Add"}
              </button>
            </form>

            <div className="project-members__list">
              <h3>
                Members ({members.length})
              </h3>
              {loading ? (
                <p>Loading members...</p>
              ) : members.length === 0 ? (
                <p>No project members yet.</p>
              ) : (
                members.map((member) => (
                  <div className="project-members__item" key={member.userId}>
                    <div className="project-members__identity">
                      <div className="project-members__avatar">
                        {member.email.charAt(0).toUpperCase() || "?"}
                      </div>
                      <div className="project-members__details">
                        <strong>{member.email || "Unknown user"}</strong>
                        <span>{member.role}</span>
                      </div>
                    </div>

                    <div className="project-members__actions">
                      <select
                        aria-label={`Role for ${member.email}`}
                        value={member.role}
                        disabled={updatingRoleUserId === member.userId}
                        onChange={(event) =>
                          void updateRole(member.userId, event.target.value)
                        }
                      >
                        <option value="Member">Member</option>
                        <option value="Admin">Admin</option>
                      </select>

                      <button
                        type="button"
                        className="project-members__remove"
                        onClick={() => void removeMember(member)}
                      >
                    <Trash2 size={13} /> Remove
                      </button>
                    </div>
                  </div>
                ))
              )}
            </div>
          </section>
        </div>,
        document.body,
      )}
    </>
  );
}
