export const TASK_STATUSES = [
  "Started",
  "In Progress",
  "On Hold",
  "Completed",
] as const;

export type TaskStatus = (typeof TASK_STATUSES)[number];

/** Convert older/API aliases to one of the status values accepted by the API. */
export function normalizeTaskStatus(status?: string | null): TaskStatus {
  const normalized = status?.trim().toLowerCase().replace(/[_-]+/g, " ");

  if (["in progress", "doing", "active"].includes(normalized ?? "")) {
    return "In Progress";
  }
  if (["on hold", "hold", "paused", "blocked"].includes(normalized ?? "")) {
    return "On Hold";
  }
  if (["completed", "complete", "done"].includes(normalized ?? "")) {
    return "Completed";
  }

  // Includes Started and legacy values such as To Do, Pending, and Open.
  return "Started";
}
