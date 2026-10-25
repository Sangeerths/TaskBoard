import { useCallback, useEffect, useRef, useState } from "react";
import { Link, useParams } from "react-router-dom";
import {
  ArrowLeft,
  ChevronRight,
  Circle,
  FolderKanban,
  ListTodo,
  MoreHorizontal,
  RefreshCw,
  X,
} from "lucide-react";
import { ApiError, api } from "../services/api";
import "./ProjectDetails.css";
import TaskForm from "../components/TaskForm";
import type { TaskAssignee } from "../components/TaskForm";
import ProjectMembers from "../components/ProjectMembers";
import type { ProjectMember } from "../components/ProjectMembers";
import TaskDiscussion from "../components/TaskDiscussion";
import ColumnActivity from "../components/ColumnActivity";
import { normalizeTaskStatus, TASK_STATUSES } from "../utils/taskStatus";

interface Project {
  projectId?: string;
  id?: string;
  name: string;
  description?: string | null;
}

interface Task {
  taskId: string;
  title: string;
  description?: string | null;
  position: number;
  status?: string;
  priority?: string;
  dueDate?: string | null;
  assigneeId?: string | null;
}

interface BoardColumn {
  columnId: string;
  name: string;
  position: number;
  tasks: Task[];
}

interface BoardResponse {
  projectId: string;
  columns: BoardColumn[];
}

function unwrapMembers(response: unknown): TaskAssignee[] {
  let members: unknown = response;

  for (let depth = 0; depth < 3; depth += 1) {
    if (Array.isArray(members)) break;
    if (typeof members !== "object" || members === null) {
      throw new Error("The members response has an unsupported format.");
    }

    const record = members as Record<string, unknown>;
    const collection = ["members", "users", "items", "data", "results"]
      .map((key) => record[key])
      .find(Array.isArray);

    if (collection) {
      members = collection;
      break;
    }

    members = record.data;
  }

  if (!Array.isArray(members)) {
    throw new Error("The members response did not contain a user list.");
  }

  return members.map((member: unknown, index: number) => {
    if (typeof member !== "object" || member === null) {
      throw new Error(`Member ${index + 1} is not a valid user record.`);
    }

    const record = member as Record<string, unknown>;
    const rawId =
      record.id ?? record.userId ?? record.memberId ?? record.assigneeId;
    if (rawId === undefined || rawId === null || String(rawId).trim() === "") {
      throw new Error(`Member ${index + 1} is missing a user ID.`);
    }

    const id = String(rawId);
    const name =
      record.displayName ??
      record.fullName ??
      record.name ??
      record.username ??
      record.email;

    return {
      id,
      label: typeof name === "string" && name.trim() ? name : id,
    };
  });
}

function unwrapProject(response: unknown): Project {
  const value = response as Record<string, unknown>;

  // Supports APIs that return the project directly or inside a wrapper.
  const candidate =
    value.project && typeof value.project === "object"
      ? (value.project as Record<string, unknown>)
      : value.data && typeof value.data === "object"
        ? (value.data as Record<string, unknown>)
        : value;

  return {
    projectId: String(candidate.projectId ?? candidate.id ?? ""),
    id: String(candidate.id ?? candidate.projectId ?? ""),
    name: String(candidate.name ?? candidate.projectName ?? "Untitled project"),
    description:
      typeof candidate.description === "string"
        ? candidate.description
        : "",
  };
}

function unwrapBoard(response: unknown): BoardResponse {
  const value = response as Record<string, unknown>;
  const candidate =
    value.board && typeof value.board === "object"
      ? (value.board as Record<string, unknown>)
      : value.data && typeof value.data === "object"
        ? (value.data as Record<string, unknown>)
        : value;

  return {
    projectId: String(candidate.projectId ?? ""),
    columns: Array.isArray(candidate.columns)
      ? (candidate.columns as BoardColumn[])
      : [],
  };
}
type TaskFormData = {
  title: string;
  description: string;
  status: string;
  priority: string;
  dueDate: string;
  assigneeId: string;
};

const emptyTaskForm: TaskFormData = {
  title: "",
  description: "",
  status: "Started",
  priority: "Low",
  dueDate: "",
  assigneeId: "",
};

function toDateTime(value: string): string | null {
  if (!value) return null;

  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date.toISOString();
}

function getColumnTone(name: string): string {
  if (/\b(done|complete|completed)\b/i.test(name)) return "is-complete";
  if (/progress|doing|active/i.test(name)) return "is-progress";
  return "is-pending";
}

function getPriorityTone(priority?: string): string {
  const normalized = priority?.toLowerCase();
  return normalized === "high" || normalized === "medium" ? normalized : "low";
}

function getTaskStatusProgress(status?: string): number | null {
  const normalized = normalizeTaskStatus(status);
  if (normalized === "Completed") return 1;
  if (normalized === "In Progress" || normalized === "On Hold") return 0.5;
  return 0;
}

function getTaskStatusTone(status?: string): string {
  const progress = getTaskStatusProgress(status);
  if (progress === 1) return "task-status-badge--complete";
  if (progress === 0.5) return "task-status-badge--progress";
  return "task-status-badge--pending";
}

export default function ProjectDetails() {
  const { id } = useParams<{ id: string }>();

  const [project, setProject] = useState<Project | null>(null);
  const [columns, setColumns] = useState<BoardColumn[]>([]);
  const [assignees, setAssignees] = useState<TaskAssignee[]>([]);
  const [assigneesLoading, setAssigneesLoading] = useState(true);
  const [assigneesError, setAssigneesError] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [boardError, setBoardError] = useState("");
  const [boardNotFound, setBoardNotFound] = useState(false);
  const [showColumnForm, setShowColumnForm] = useState(false);
const [columnName, setColumnName] = useState("");
const [creatingColumn, setCreatingColumn] = useState(false);
const [columnError, setColumnError] = useState("");
const [editingColumnId, setEditingColumnId] = useState<string | null>(null);
const [editingColumnName, setEditingColumnName] = useState("");
const [updatingColumn, setUpdatingColumn] = useState(false);
const [deletingColumnId, setDeletingColumnId] = useState<string | null>(null);
const [columnActionMenuId, setColumnActionMenuId] = useState<string | null>(null);
const columnActionsMenuRef = useRef<HTMLDivElement | null>(null);
const taskActionsMenuRef = useRef<HTMLDivElement | null>(null);
const [reordering, setReordering] = useState(false);
const [taskFormColumnId, setTaskFormColumnId] = useState<string | null>(null);
const [editingTaskId, setEditingTaskId] = useState<string | null>(null);
const [collapsedColumnIds, setCollapsedColumnIds] = useState<Set<string>>(
  () => new Set(),
);
const [discussionTaskId, setDiscussionTaskId] = useState<string | null>(null);
const [activityColumnId, setActivityColumnId] = useState<string | null>(null);
const [taskActionMenuId, setTaskActionMenuId] = useState<string | null>(null);
const [taskMoveMenuId, setTaskMoveMenuId] = useState<string | null>(null);
const [savingTask, setSavingTask] = useState(false);
const [updatingTaskStatusId, setUpdatingTaskStatusId] = useState<string | null>(null);
const [taskError, setTaskError] = useState("");
const [taskNotice, setTaskNotice] = useState("");
const [activityRefreshKey, setActivityRefreshKey] = useState(0);
const [draggedTask, setDraggedTask] = useState<{
  taskId: string;
  sourceColumnId: string;
} | null>(null);

const [draggedColumnId, setDraggedColumnId] = useState<string | null>(null);
const [dragOverColumnId, setDragOverColumnId] = useState<string | null>(null);
const refreshColumnActivity = useCallback(() => {
  setActivityRefreshKey((key) => key + 1);
}, []);
const handleMembersChange = useCallback((members: ProjectMember[]) => {
  setAssignees(
    members.map((member) => ({
      id: member.userId,
      label: member.email || member.userId,
    })),
  );
}, []);
useEffect(() => {
  if (!columnActionMenuId) return;

  function closeMenuOnOutsideClick(event: PointerEvent) {
    if (
      event.target instanceof Node &&
      !columnActionsMenuRef.current?.contains(event.target)
    ) {
      setColumnActionMenuId(null);
    }
  }

  document.addEventListener("pointerdown", closeMenuOnOutsideClick);
  return () =>
    document.removeEventListener("pointerdown", closeMenuOnOutsideClick);
}, [columnActionMenuId]);
useEffect(() => {
  if (!taskActionMenuId) return;

  function closeTaskMenuOnOutsideClick(event: PointerEvent) {
    if (
      event.target instanceof Node &&
      !taskActionsMenuRef.current?.contains(event.target)
    ) {
      setTaskActionMenuId(null);
      setTaskMoveMenuId(null);
    }
  }

  document.addEventListener("pointerdown", closeTaskMenuOnOutsideClick);
  return () =>
    document.removeEventListener("pointerdown", closeTaskMenuOnOutsideClick);
}, [taskActionMenuId]);
async function reorderColumnTo(
  draggedId: string,
  targetId: string,
) {
  if (draggedId === targetId || reordering || !id) return;

  const sorted = [...columns].sort(
    (a, b) => a.position - b.position,
  );

  const fromIndex = sorted.findIndex(
    (column) => column.columnId === draggedId,
  );
  const toIndex = sorted.findIndex(
    (column) => column.columnId === targetId,
  );

  if (fromIndex < 0 || toIndex < 0) return;

  const reordered = [...sorted];
  const [movedColumn] = reordered.splice(fromIndex, 1);
  reordered.splice(toIndex, 0, movedColumn);

  const previousColumns = columns;

  setColumns(
    reordered.map((column, index) => ({
      ...column,
      position: index,
    })),
  );

  setReordering(true);
  setColumnError("");

  try {
    await api.put(`api/Board/${id}/columns/reorder`, {
      columnIds: reordered.map((column) => column.columnId),
    });

    await loadProject();
  } catch (err) {
    setColumns(previousColumns);
    setColumnError(
      err instanceof Error
        ? err.message
        : "Failed to reorder columns.",
    );
  } finally {
    setReordering(false);
    setDraggedColumnId(null);
  }
}
  const loadProject = useCallback(async (
    showLoading = true,
    signal?: AbortSignal,
  ) => {
    if (signal?.aborted) return;

    if (!id) {
      setError("Project ID is missing from the URL.");
      setLoading(false);
      return;
    }

    setLoading(showLoading);
    setAssigneesLoading(true);
    setError("");
    setBoardError("");
    setBoardNotFound(false);
    setAssigneesError("");

    try {
      const projectId = encodeURIComponent(id);
      const [projectResult, boardResult, membersResult] = await Promise.allSettled([
        api.get<unknown>(`api/projects/${projectId}`, { signal }),
        api.get<unknown>(`api/Board/${projectId}/board`, { signal }),
        api.get<unknown>(`api/Projects/${projectId}/members`, { signal }),
      ]);

      if (signal?.aborted) return;

      if (projectResult.status === "rejected") {
        throw projectResult.reason;
      }

      setProject(unwrapProject(projectResult.value));

      if (membersResult.status === "fulfilled") {
        try {
          setAssignees(unwrapMembers(membersResult.value));
        } catch (err) {
          setAssignees([]);
          setAssigneesError(
            err instanceof Error
              ? err.message
              : "The members response could not be read.",
          );
        }
      } else {
        setAssignees([]);
        setAssigneesError(
          membersResult.reason instanceof Error
            ? membersResult.reason.message
            : "Unable to load project members.",
        );
      }

      if (boardResult.status === "fulfilled") {
        setColumns(unwrapBoard(boardResult.value).columns);
      } else {
        setColumns([]);
        if (
          boardResult.reason instanceof ApiError &&
          boardResult.reason.status === 404
        ) {
          setBoardNotFound(true);
        } else {
          setBoardError(
            boardResult.reason instanceof Error
              ? boardResult.reason.message
              : "Unable to load this project's board.",
          );
        }
      }
    } catch (err) {
      if (signal?.aborted) return;
      setError(
        err instanceof Error
          ? err.message
          : "Unable to load this project."
      );
    } finally {
      if (!signal?.aborted) {
        setLoading(false);
        setAssigneesLoading(false);
      }
    }
  }, [id]);

  useEffect(() => {
    const controller = new AbortController();
    void loadProject(true, controller.signal);
    return () => controller.abort();
  }, [loadProject]);

  useEffect(() => {
    if (!taskFormColumnId) return;

    function closeOnEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setTaskFormColumnId(null);
        setTaskError("");
      }
    }

    document.addEventListener("keydown", closeOnEscape);
    return () => document.removeEventListener("keydown", closeOnEscape);
  }, [taskFormColumnId]);

  if (loading) {
    return (
      <main className="project-details-page">
        <p className="details-status">
          <RefreshCw size={18} className="spin" />
          Loading project...
        </p>
      </main>
    );
  }

  if (error || !project) {
    return (
      <main className="project-details-page">
        <Link to="/projects" className="back-link">
          <ArrowLeft size={17} /> Back to projects
        </Link>

        <div className="details-message error-message">
          <h2>Couldn't load the project</h2>
          <p>{error || "Project not found."}</p>
          <button onClick={() => void loadProject()}>Try again</button>
        </div>
      </main>
    );
  }

  const editingTask = editingTaskId
    ? columns
        .flatMap((column) => column.tasks ?? [])
        .find((task) => task.taskId === editingTaskId)
    : undefined;

  return (
    <main className="project-details-page">
      <div className="workspace-topbar">
        <Link to="/projects" className="back-link">
          <ArrowLeft size={17} /> All projects
        </Link>
        <span className="workspace-topbar-label">PROJECT WORKSPACE</span>
      </div>

      <header className="project-details-header">
        <div className="project-details-title">
          <span className="project-details-mark"><FolderKanban size={23} /></span>
          <div>
            <p className="eyebrow">YOUR PROJECT</p>
            <h1>{project.name}</h1>
            <p className="project-description">
              {project.description || "No description added yet."}
            </p>
          </div>
        </div>

        <ProjectMembers
          projectId={id!}
          projectName={project.name}
          onMembersChange={handleMembersChange}
        />
      </header>

      <section className="board-section">
        <div className="board-heading">
          <div>
            <h2>Project board</h2>
            <p>Group tasks into workflow stages.</p>
          </div>
          <button
  className="add-column-button"
  onClick={() => {
    setShowColumnForm((current) => !current);
    setColumnError("");
  }}
>
  + Add workflow stage
</button>
        </div>
{columnError && !editingColumnId && !showColumnForm && (
  <p className="column-error" role="alert">
    {columnError}
  </p>
)}
{taskError && (
  <p className="task-error" role="alert">
    {taskError}
  </p>
)}
{taskNotice && (
  <p className="task-success" role="status">
    {taskNotice}
  </p>
)}
{boardError ? (
          <div className="details-message error-message">
            <h3>Couldn't load the project board</h3>
            <p>{boardError}</p>
            <button onClick={() => void loadProject()}>Try again</button>
          </div>
        ) : columns.length === 0 ? (
          <div className="empty-board">
            <div className="empty-board-icon">
              <ListTodo size={28} />
            </div>
            <h3>
              {boardNotFound ? "Board not found" : "Your board is empty"}
            </h3>
            <p>
              {boardNotFound
                ? "The API returned 404 for this project's board. Project details loaded, but no board data is available."
                : "The project loaded successfully, but there are no columns to display yet."}
            </p>
          </div>
        ) : (
          <div className="kanban-board">
            {columns
              .slice()
              .sort((a, b) => a.position - b.position)
              .map((column) => {
                const isExpanded = !collapsedColumnIds.has(column.columnId);
                const columnTone = getColumnTone(column.name).replace("is-", "");
                return (
                <section
  className={`kanban-column workflow-stage workflow-stage--${columnTone} ${
    dragOverColumnId === column.columnId ? "drag-over" : ""
  } ${
    isExpanded ? "is-expanded" : ""
  }`}
  key={column.columnId}
  draggable={editingColumnId !== column.columnId && !reordering}
  onDragStart={(event) => {
    if ((event.target as HTMLElement).closest(".task-card")) {
      event.preventDefault();
      return;
    }

    if (event.target !== event.currentTarget) {
      event.preventDefault();
      return;
    }

    setDraggedColumnId(column.columnId);
    event.dataTransfer.effectAllowed = "move";
    event.dataTransfer.setData("text/plain", column.columnId);
  }}
  onDragOver={(event) => {
    if (draggedTask) {
      event.preventDefault();
      if (collapsedColumnIds.has(column.columnId)) {
        setCollapsedColumnIds((current) => {
          const next = new Set(current);
          next.delete(column.columnId);
          return next;
        });
      }
      setDragOverColumnId(column.columnId);
      return;
    }

    if (!draggedColumnId || draggedColumnId === column.columnId) return;

    event.preventDefault();
    event.dataTransfer.dropEffect = "move";
  }}
  onDrop={(event) => {
    if (draggedTask) return;

    event.preventDefault();

    const sourceId =
      draggedColumnId || event.dataTransfer.getData("text/plain");

    setDraggedColumnId(null);

    if (sourceId) {
      void reorderColumnTo(sourceId, column.columnId);
    }
  }}
  onDragEnd={() => setDraggedColumnId(null)}
>
                  <div className="column-title">
                    <button
                      type="button"
                      className={`column-title__toggle ${
                        isExpanded ? "is-open" : ""
                      }`}
                      aria-expanded={isExpanded}
                      onClick={() => setCollapsedColumnIds((current) => {
                        const next = new Set(current);
                        if (next.has(column.columnId)) next.delete(column.columnId);
                        else next.add(column.columnId);
                        return next;
                      })}
                    >
                      <Circle
                        size={11}
                        className={`column-status ${getColumnTone(column.name)}`}
                      />
                      <span>{column.name}</span>
                      <span className="column-title__count">
                        {column.tasks?.length ?? 0}
                      </span>
                      <ChevronRight size={15} className="column-title__chevron" />
                    </button>

                    <div
                      className="column-actions-menu"
                      ref={
                        columnActionMenuId === column.columnId
                          ? columnActionsMenuRef
                          : null
                      }
                    >
                      <button
                        type="button"
                        className="column-actions-menu__trigger"
                        aria-label={`Actions for ${column.name}`}
                        aria-haspopup="menu"
                        aria-expanded={columnActionMenuId === column.columnId}
                        onClick={() =>
                          setColumnActionMenuId((current) =>
                            current === column.columnId ? null : column.columnId,
                          )
                        }
                      >
                        <MoreHorizontal size={19} />
                      </button>
                      {columnActionMenuId === column.columnId && (
                        <div
                          className="column-actions-menu__list"
                          role="menu"
                          aria-label={`Actions for ${column.name}`}
                        >
                          <button
                            type="button"
                            role="menuitem"
                            onClick={() => {
                              setEditingColumnId(column.columnId);
                              setEditingColumnName(column.name);
                              setColumnActionMenuId(null);
                              setColumnError("");
                            }}
                          >
                            Edit stage
                          </button>
                          <button
                            type="button"
                            role="menuitem"
                            className="delete-column-action"
                            disabled={deletingColumnId === column.columnId}
                            onClick={() => {
                              setColumnActionMenuId(null);
                              void deleteColumn(column);
                            }}
                          >
                            {deletingColumnId === column.columnId
                              ? "Deleting..."
                              : "Delete stage"}
                          </button>
                        </div>
                      )}
                    </div>

</div>

{isExpanded && (
  <>
<button
  type="button"
  className="add-task-button"
  onClick={() => {
    setTaskFormColumnId(column.columnId);
    setEditingTaskId(null);
    setTaskError("");
  }}
>
  + Add task
</button>

<div
  className="task-list"
  onDragOver={(event) => {
    if (!draggedTask) return;

    event.preventDefault();
    event.stopPropagation();
    setDragOverColumnId(column.columnId);
    event.dataTransfer.dropEffect = "move";
  }}
  onDragLeave={(event) => {
    if (!event.currentTarget.contains(event.relatedTarget as Node)) {
      setDragOverColumnId(null);
    }
  }}
  onDrop={(event) => {
    if (!draggedTask) return;

    event.preventDefault();
    event.stopPropagation();

    const sourceTask = draggedTask;
    const destinationColumnId = column.columnId;
    const position = (column.tasks ?? []).length;

    setDraggedTask(null);
    setDragOverColumnId(null);

    void moveTask(sourceTask.taskId, destinationColumnId, position);
  }}
>
  {(column.tasks ?? [])
    .slice()
    .sort((a, b) => a.position - b.position)
    .map((task) => (
      <article
  className={`task-card task-card--${getPriorityTone(task.priority)} ${
    draggedTask?.taskId === task.taskId ? "task-dragging" : ""
  }`}
  key={task.taskId}
  draggable
  onDragStart={(event) => {
    event.stopPropagation();

    setDraggedTask({
      taskId: task.taskId,
      sourceColumnId: column.columnId,
    });

    event.dataTransfer.effectAllowed = "move";
    event.dataTransfer.setData("text/plain", task.taskId);
  }}
  onDragOver={(event) => {
    if (!draggedTask || draggedTask.taskId === task.taskId) return;

    event.preventDefault();
    event.stopPropagation();
    event.dataTransfer.dropEffect = "move";
  }}
  onDrop={(event) => {
    if (!draggedTask || draggedTask.taskId === task.taskId) return;

    event.preventDefault();
    event.stopPropagation();

    const sourceTask = draggedTask;
    setDraggedTask(null);
    setDragOverColumnId(null);

    void moveTask(
      sourceTask.taskId,
      column.columnId,
      task.position,
    );
  }}
  onDragEnd={() => {
    setDraggedTask(null);
    setDragOverColumnId(null);
  }}
>
        <div className="task-card__heading">
          <h4>{task.title}</h4>
          <div
            className="task-card__menu-wrap"
            ref={taskActionMenuId === task.taskId ? taskActionsMenuRef : null}
          >
            <button
              type="button"
              className="task-card__menu-trigger"
              aria-label={`Task actions for ${task.title}`}
              aria-haspopup="menu"
              aria-expanded={taskActionMenuId === task.taskId}
              onClick={() =>
                {
                  setTaskActionMenuId((current) => {
                    const next =
                      current === task.taskId ? null : task.taskId;
                    if (next === null) setTaskMoveMenuId(null);
                    return next;
                  });
                }
              }
            >
              <MoreHorizontal size={19} />
            </button>
            {taskActionMenuId === task.taskId && (
              <div
                className="task-card__menu"
                role="menu"
                aria-label={`Actions for ${task.title}`}
              >
                <button
                  type="button"
                  role="menuitem"
                  onClick={() => {
                    setTaskActionMenuId(null);
                    setTaskMoveMenuId(null);
                    setEditingTaskId(task.taskId);
                    setTaskFormColumnId(null);
                    setTaskError("");
                  }}
                >
                  Edit
                </button>
                <button
                  type="button"
                  role="menuitem"
                  onClick={() => {
                    setTaskActionMenuId(null);
                    setTaskMoveMenuId(null);
                    void deleteTask(task.taskId);
                  }}
                >
                  Delete
                </button>
                <div className="task-card__move-wrap">
                  <button
                    type="button"
                    role="menuitem"
                    aria-haspopup="menu"
                    aria-expanded={taskMoveMenuId === task.taskId}
                    onClick={() =>
                      setTaskMoveMenuId((current) =>
                        current === task.taskId ? null : task.taskId,
                      )
                    }
                  >
                    <span>Move to</span>
                    <ChevronRight size={14} />
                  </button>
                  {taskMoveMenuId === task.taskId && (
                    <div
                      className="task-card__move-menu"
                      role="menu"
                      aria-label={`Move ${task.title} to column`}
                    >
                      {columns
                        .filter(
                          (targetColumn) =>
                            targetColumn.columnId !== column.columnId,
                        )
                        .map((targetColumn) => (
                          <button
                            key={targetColumn.columnId}
                            type="button"
                            role="menuitem"
                            onClick={() => {
                              setTaskMoveMenuId(null);
                              setTaskActionMenuId(null);
                              void moveTask(
                                task.taskId,
                                targetColumn.columnId,
                                0,
                              );
                            }}
                          >
                            {targetColumn.name}
                          </button>
                        ))}
                      {columns.length <= 1 && (
                        <span className="task-card__move-empty">
                          No other columns
                        </span>
                      )}
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
        </div>

        {task.description && <p>{task.description}</p>}

        <div className="task-meta">
          <span className={`task-status-badge ${getTaskStatusTone(task.status)}`}>
            {normalizeTaskStatus(task.status)}
          </span>
          <span
            className={`priority priority-${getPriorityTone(task.priority)}`}
          >
            {task.priority ?? "Low"}
          </span>

          {task.dueDate && (
            <span>
              Due: {new Date(task.dueDate).toLocaleDateString()}
            </span>
          )}
        </div>

        <label className="task-status-control">
          <span>Status</span>
          <select
            className={`task-status-select ${getTaskStatusTone(task.status)}`}
            aria-label={`Update status for ${task.title}`}
            value={normalizeTaskStatus(task.status)}
            disabled={updatingTaskStatusId === task.taskId}
            onChange={(event) => void updateTaskStatus(task, event.target.value)}
          >
            {TASK_STATUSES.map((status) => (
              <option key={status} value={status}>{status}</option>
            ))}
          </select>
          {updatingTaskStatusId === task.taskId && (
            <span className="task-status-saving" role="status">Saving status…</span>
          )}
        </label>

        <button
          type="button"
          className="task-discussion-toggle"
          aria-expanded={discussionTaskId === task.taskId}
          onClick={() => setDiscussionTaskId((current) =>
            current === task.taskId ? null : task.taskId,
          )}
        >
          {discussionTaskId === task.taskId ? "Hide comments" : "Comments"}
        </button>
        {discussionTaskId === task.taskId && (
          <TaskDiscussion
            taskId={task.taskId}
            onCommentChange={refreshColumnActivity}
          />
        )}
      </article>
    ))}

                    {(!column.tasks || column.tasks.length === 0) && (
                      <p className="empty-column">No tasks yet</p>
                    )}
                  </div>
                  <button
                    type="button"
                    className="column-activity-toggle"
                    aria-expanded={activityColumnId === column.columnId}
                    onClick={() => setActivityColumnId((current) =>
                      current === column.columnId ? null : column.columnId,
                    )}
                  >
                    {activityColumnId === column.columnId ? "Hide activity" : "View activity"}
                  </button>
                  {activityColumnId === column.columnId && (
                    <ColumnActivity
                      columnId={column.columnId}
                      tasks={column.tasks ?? []}
                      refreshKey={activityRefreshKey}
                    />
                  )}
                  </>
                )}
                </section>
              );})}
          </div>
        )}


      </section>
      {(taskFormColumnId || editingTaskId) && (
        <div
          className="task-modal-backdrop"
          role="presentation"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget) {
              setTaskFormColumnId(null);
              setEditingTaskId(null);
              setTaskError("");
            }
          }}
        >
          <section
            className="task-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="task-modal-title"
          >
            <div className="task-modal__heading">
              <div>
                <h2 id="task-modal-title">
                  {editingTask
                    ? "Edit task"
                    : `Add task to ${
                        columns.find(
                          (column) =>
                            column.columnId === taskFormColumnId,
                        )?.name ?? "column"
                      }`}
                </h2>
                <p>
                  {editingTask
                    ? "Update the task details below."
                    : "Enter the task details below."}
                </p>
              </div>
              <button
                type="button"
                className="task-modal__close"
                aria-label="Close task dialog"
                onClick={() => {
                  setTaskFormColumnId(null);
                  setEditingTaskId(null);
                  setTaskError("");
                }}
              >
                <X size={18} />
              </button>
            </div>
            {taskError && (
              <p className="task-error" role="alert">
                {taskError}
              </p>
            )}
            <TaskForm
              key={editingTask?.taskId ?? taskFormColumnId}
              initialValues={
                editingTask
                  ? {
                      title: editingTask.title,
                      description: editingTask.description ?? "",
                      status: normalizeTaskStatus(editingTask.status),
                      priority: editingTask.priority ?? "Low",
                      dueDate: editingTask.dueDate
                        ? new Date(editingTask.dueDate)
                            .toISOString()
                            .slice(0, 16)
                        : "",
                      assigneeId: editingTask.assigneeId ?? "",
                    }
                  : emptyTaskForm
              }
              assignees={assignees}
              assigneesLoading={assigneesLoading}
              assigneesError={assigneesError}
              saving={savingTask}
              onSubmit={(values) => {
                if (editingTask) {
                  void updateTask(editingTask.taskId, values);
                } else if (taskFormColumnId) {
                  void createTask(taskFormColumnId, values);
                }
              }}
              onCancel={() => {
                setTaskFormColumnId(null);
                setEditingTaskId(null);
                setTaskError("");
              }}
            />
          </section>
        </div>
      )}
      {editingColumnId && (
        <div
          className="task-modal-backdrop"
          role="presentation"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget && !updatingColumn) {
              setEditingColumnId(null);
              setEditingColumnName("");
              setColumnError("");
            }
          }}
        >
          <section
            className="column-edit-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="column-edit-title"
          >
            <div className="task-modal__heading">
              <div>
                <h2 id="column-edit-title">Edit workflow stage</h2>
                <p>Choose a clear name for this stage.</p>
              </div>
              <button
                type="button"
                className="task-modal__close"
                aria-label="Close edit stage dialog"
                disabled={updatingColumn}
                onClick={() => {
                  setEditingColumnId(null);
                  setEditingColumnName("");
                  setColumnError("");
                }}
              >
                <X size={18} />
              </button>
            </div>
            {columnError && (
              <p className="column-error" role="alert">
                {columnError}
              </p>
            )}
            <form
              className="column-edit-form"
              onSubmit={(event) => {
                event.preventDefault();
                void updateColumn(editingColumnId);
              }}
            >
              <label htmlFor="column-edit-name">Stage name</label>
              <input
                id="column-edit-name"
                value={editingColumnName}
                onChange={(event) => setEditingColumnName(event.target.value)}
                maxLength={100}
                required
                autoFocus
              />
              <div className="column-edit-form__actions">
                <button
                  type="button"
                  onClick={() => {
                    setEditingColumnId(null);
                    setEditingColumnName("");
                    setColumnError("");
                  }}
                  disabled={updatingColumn}
                >
                  Cancel
                </button>
                <button type="submit" disabled={updatingColumn}>
                  {updatingColumn ? "Saving..." : "Save changes"}
                </button>
              </div>
            </form>
          </section>
        </div>
      )}
      {showColumnForm && (
        <div
          className="task-modal-backdrop"
          role="presentation"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget && !creatingColumn) {
              setShowColumnForm(false);
              setColumnName("");
              setColumnError("");
            }
          }}
        >
          <section
            className="column-edit-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="column-create-title"
          >
            <div className="task-modal__heading">
              <div>
                <h2 id="column-create-title">Add workflow stage</h2>
                <p>Create a stage to organize work on this board.</p>
              </div>
              <button
                type="button"
                className="task-modal__close"
                aria-label="Close add stage dialog"
                disabled={creatingColumn}
                onClick={() => {
                  setShowColumnForm(false);
                  setColumnName("");
                  setColumnError("");
                }}
              >
                <X size={18} />
              </button>
            </div>
            {columnError && (
              <p className="column-error" role="alert">
                {columnError}
              </p>
            )}
            <form
              className="column-edit-form"
              onSubmit={(event) => {
                event.preventDefault();
                void createColumn();
              }}
            >
              <label htmlFor="column-create-name">Stage name</label>
              <input
                id="column-create-name"
                type="text"
                placeholder="e.g. Ready for review"
                value={columnName}
                onChange={(event) => setColumnName(event.target.value)}
                maxLength={100}
                required
                autoFocus
              />
              <div className="column-edit-form__actions">
                <button
                  type="button"
                  disabled={creatingColumn}
                  onClick={() => {
                    setShowColumnForm(false);
                    setColumnName("");
                    setColumnError("");
                  }}
                >
                  Cancel
                </button>
                <button type="submit" disabled={creatingColumn}>
                  {creatingColumn ? "Creating..." : "Create stage"}
                </button>
              </div>
            </form>
          </section>
        </div>
)}
    </main>
  );

  async function createColumn() {
  if (!id || !columnName.trim()) {
    setColumnError("Please enter a column name.");
    return;
  }

  setCreatingColumn(true);
  setColumnError("");

  try {
    await api.post(`api/Board/${id}/columns`, {
      name: columnName.trim(),
    });

    setColumnName("");
    setShowColumnForm(false);

    // Reload the board to display the newly created column.
    await loadProject();
  } catch (err) {
    setColumnError(
      err instanceof Error ? err.message : "Failed to create column."
    );
  } finally {
    setCreatingColumn(false);
  }
}

async function updateColumn(columnId: string) {
  if (!editingColumnName.trim()) {
    setColumnError("Please enter a column name.");
    return;
  }

  setUpdatingColumn(true);
  setColumnError("");

  try {
    await api.put(`api/columns/${columnId}`, {
      name: editingColumnName.trim(),
    });

    setEditingColumnId(null);
    setEditingColumnName("");

    await loadProject();
  } catch (err) {
    setColumnError(
      err instanceof Error ? err.message : "Failed to update column."
    );
  } finally {
    setUpdatingColumn(false);
  }
}
async function deleteColumn(column: BoardColumn) {
  const confirmed = window.confirm(
    "Are you sure you want to delete this column? This action cannot be undone."
  );

  if (!confirmed) return;

  setDeletingColumnId(column.columnId);
  setColumnError("");

  try {
    await api.delete(`api/columns/${encodeURIComponent(column.columnId)}`);

    await loadProject();
  } catch (err) {
    setColumnError(
      err instanceof Error ? err.message : "Failed to delete column."
    );
  } finally {
    setDeletingColumnId(null);
  }
}
async function createTask(columnId: string, values: TaskFormData) {
if (!values.title.trim()) {
  setTaskError("Task title is required.");
  return;
}

  setSavingTask(true);
  setTaskError("");

  try {
    await api.post(`api/Tasks/columns/${columnId}/tasks`, {
      title: values.title.trim(),
      description: values.description.trim(),
      status: values.status,
      priority: values.priority,
      dueDate: toDateTime(values.dueDate),
      assigneeId: values.assigneeId.trim() || null,
    });

    setTaskFormColumnId(null);

    await loadProject();
  } catch (err) {
    setTaskError(
      err instanceof Error ? err.message : "Failed to create task."
    );
  } finally {
    setSavingTask(false);
  }
}
async function updateTask(taskId: string, values: TaskFormData) {
  if (!values.title.trim()) {
    setTaskError("Task title is required.");
    return;
  }

  setSavingTask(true);
  setTaskError("");

  try {
    await api.put(`api/Tasks/tasks/${taskId}`, {
      title: values.title.trim(),
      description: values.description.trim(),
      status: values.status,
      priority: values.priority,
      dueDate: toDateTime(values.dueDate),
      assigneeId: values.assigneeId.trim() || null,
    });

    setEditingTaskId(null);

    await loadProject();
  } catch (err) {
    setTaskError(
      err instanceof Error ? err.message : "Failed to update task."
    );
  } finally {
    setSavingTask(false);
  }
}
async function updateTaskStatus(task: Task, status: string) {
  if (status === task.status || updatingTaskStatusId) return;

  setUpdatingTaskStatusId(task.taskId);
  setTaskError("");
  setTaskNotice("");
  try {
    await api.put(`api/Tasks/tasks/${encodeURIComponent(task.taskId)}`, {
      title: task.title,
      description: task.description ?? "",
      status,
      priority: task.priority ?? "Low",
      dueDate: task.dueDate ?? null,
      assigneeId: task.assigneeId ?? null,
    });
    await loadProject(false);
    setTaskNotice(`Task status updated to ${status}.`);
  } catch (err) {
    setTaskError(
      err instanceof Error ? err.message : "Failed to update task status.",
    );
  } finally {
    setUpdatingTaskStatusId(null);
  }
}
async function deleteTask(taskId: string) {
  if (!window.confirm("Are you sure you want to delete this task?")) {
    return;
  }

  setTaskError("");

  try {
    await api.delete(`api/Tasks/tasks/${taskId}`);
    await loadProject();
  } catch (err) {
    setTaskError(
      err instanceof Error ? err.message : "Failed to delete task."
    );
  }
}
async function moveTask(
  taskId: string,
  destinationColumnId: string,
  position: number,
  boardColumns: BoardColumn[] = columns,
) {
  const destinationColumn = boardColumns.find(
    (column) => column.columnId === destinationColumnId,
  );
  if (!destinationColumn) {
    setTaskError("The selected workflow stage could not be found.");
    return;
  }

  const previousColumns = boardColumns;
  const taskToMove = boardColumns
    .flatMap((column) => column.tasks ?? [])
    .find((task) => task.taskId === taskId);
  if (!taskToMove) {
    setTaskError("The task could not be found on this board.");
    return;
  }
  setTaskNotice("");
  setTaskError("");

  setColumns((current) => {
    const withoutTask = current.map((column) => {
      const remaining = (column.tasks ?? []).filter((task) => task.taskId !== taskId);
      return { ...column, tasks: remaining };
    });

    const target = withoutTask.find((column) => column.columnId === destinationColumnId);
    if (!target) return current;
    const targetTasks = [...(target.tasks ?? [])];
    targetTasks.splice(Math.max(0, Math.min(position, targetTasks.length)), 0, taskToMove);
    return withoutTask.map((column) =>
      column.columnId === destinationColumnId
        ? { ...column, tasks: targetTasks.map((task, index) => ({ ...task, position: index })) }
        : column,
    );
  });

  try {
    await api.put(`api/Tasks/tasks/${taskId}/move`, {
      columnId: destinationColumnId,
      position,
    });

    await loadProject(false);
    setTaskNotice(`Status updated to ${destinationColumn.name}.`);
  } catch (err) {
    setColumns(previousColumns);
    setTaskError(
      err instanceof Error ? err.message : "Failed to move task."
    );
  }
}

}
