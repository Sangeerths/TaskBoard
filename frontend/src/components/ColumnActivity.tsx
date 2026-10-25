import { useCallback, useEffect, useState } from "react";
import { api } from "../services/api";

interface ColumnActivityProps {
  columnId: string;
  tasks: { taskId: string; title: string }[];
  refreshKey: number;
}

interface ActivityEntry {
  id: string;
  taskTitle: string;
  action: string;
  description: string;
  createdAt?: string;
  userName?: string;
}

function getList(response: unknown): Record<string, unknown>[] {
  let value: unknown = response;

  for (let depth = 0; depth < 3; depth += 1) {
    if (Array.isArray(value)) break;
    if (typeof value !== "object" || value === null) return [];

    const record = value as Record<string, unknown>;
    const list = [record.activities, record.activity, record.items, record.data, record.results]
      .find(Array.isArray);
    if (list) {
      value = list;
      break;
    }
    return [];
  }

  return Array.isArray(value)
    ? value.filter(
        (item): item is Record<string, unknown> =>
          item !== null && typeof item === "object",
      )
    : [];
}

function getText(record: Record<string, unknown>, ...keys: string[]): string {
  for (const key of keys) {
    const value = record[key];
    if (typeof value === "string" && value.trim()) return value;
  }
  return "";
}

function getDate(record: Record<string, unknown>): string | undefined {
  const value =
    record.createdAt ??
    record.CreatedAt ??
    record.timestamp ??
    record.Timestamp ??
    record.createdOn ??
    record.CreatedOn;

  if (typeof value === "string" && value) return value;
  if (typeof value === "number") {
    const date = new Date(value < 1e12 ? value * 1000 : value);
    return Number.isNaN(date.getTime()) ? undefined : date.toISOString();
  }
  return undefined;
}

function formatDate(value?: string): string {
  if (!value) return "";
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleString(undefined, {
        dateStyle: "medium",
        timeStyle: "short",
      });
}

export default function ColumnActivity({
  columnId,
  tasks,
  refreshKey,
}: ColumnActivityProps) {
  const [entries, setEntries] = useState<ActivityEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadActivity = useCallback(async () => {
    setLoading(true);
    setError("");

    const results: { activities: ActivityEntry[]; error: string }[] = [];
    // Each task has its own activity endpoint. Limit concurrent requests so
    // large columns do not flood the API when the board first opens.
    for (let start = 0; start < tasks.length; start += 4) {
      const batch = await Promise.all(
        tasks.slice(start, start + 4).map(async (task) => {
        try {
          const response = await api.get<unknown>(
            `api/Activity/tasks/${encodeURIComponent(task.taskId)}/activity`,
          );
          const activities = getList(response).map((record, index) => ({
            id:
              getText(record, "activityId", "ActivityId", "id", "Id") ||
              `${task.taskId}-${index}`,
            taskTitle: task.title,
            action: getText(record, "action", "Action", "type", "Type"),
            description: getText(
              record,
              "description",
              "Description",
              "message",
              "Message",
              "details",
              "Details",
            ),
            createdAt: getDate(record),
            userName: getText(
              record,
              "userEmail",
              "UserEmail",
              "userName",
              "UserName",
            ),
          }));
          return { activities, error: "" };
        } catch (reason) {
          return {
            activities: [],
            error:
              reason instanceof Error
                ? reason.message
                : `Could not load activity for ${task.title}.`,
          };
        }
        }),
      );
      results.push(...batch);
    }

    setEntries(
      results
        .flatMap((result) => result.activities)
        .sort((first, second) => {
          const firstTime = first.createdAt
            ? new Date(first.createdAt).getTime()
            : 0;
          const secondTime = second.createdAt
            ? new Date(second.createdAt).getTime()
            : 0;
          return secondTime - firstTime;
        }),
    );

    const failures = results.filter((result) => result.error);
    if (failures.length > 0) {
      setError(
        failures.length === tasks.length
          ? failures[0].error
          : `Activity could not be loaded for ${failures.length} task(s).`,
      );
    }
    setLoading(false);
  }, [tasks]);

  useEffect(() => {
    void Promise.resolve().then(() => loadActivity());
  }, [loadActivity, columnId, refreshKey]);

  return (
    <section className="column-activity" aria-label="Column activity">
      <div className="column-activity__heading">
        <h4>Column activity</h4>
        <button
          type="button"
          onClick={() => void loadActivity()}
          disabled={loading || tasks.length === 0}
        >
          Refresh
        </button>
      </div>
      {loading ? (
        <p className="task-discussion__muted">Loading activity...</p>
      ) : (
        <>
          {error && (
            <p className="task-discussion__notice task-discussion__notice--error">
              {error}
            </p>
          )}
          {entries.length === 0 ? (
            <p className="task-discussion__muted">No activity recorded.</p>
          ) : (
            <div className="task-discussion__timeline">
              {entries.map((entry) => (
                <article className="task-discussion__event" key={entry.id}>
                  <span className="task-discussion__dot" />
                  <div>
                    <strong>
                      {entry.description || entry.action || "Task activity"}
                    </strong>
                    <p>Task: {entry.taskTitle}</p>
                    {entry.userName && <p>{entry.userName}</p>}
                    {entry.createdAt && (
                      <time dateTime={entry.createdAt}>
                        {formatDate(entry.createdAt)}
                      </time>
                    )}
                  </div>
                </article>
              ))}
            </div>
          )}
        </>
      )}
    </section>
  );
}
