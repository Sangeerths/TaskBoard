import type { FormEvent } from "react";
import { useState } from "react";
import { normalizeTaskStatus, TASK_STATUSES } from "../utils/taskStatus";
export interface TaskFormValues {
  title: string;
  description: string;
  status: string;
  priority: string;
  dueDate: string;
  assigneeId: string;
}

export interface TaskAssignee {
  id: string;
  label: string;
}

interface TaskFormProps {
  initialValues: TaskFormValues;
  assignees: TaskAssignee[];
  assigneesLoading: boolean;
  assigneesError: string;
  saving: boolean;
  onSubmit: (values: TaskFormValues) => void;
  onCancel: () => void;
}

export default function TaskForm({
  initialValues,
  assignees,
  assigneesLoading,
  assigneesError,
  saving,
  onSubmit,
  onCancel,
}: TaskFormProps) {
  const [form, setForm] = useState({
    ...initialValues,
    status: normalizeTaskStatus(initialValues.status),
  });

  function updateField<Key extends keyof TaskFormValues>(
    field: Key,
    value: TaskFormValues[Key],
  ) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onSubmit({ ...form, status: normalizeTaskStatus(form.status) });
  }

  return (
    <form className="task-form" onSubmit={submit}>
      <label>
        Task title *
        <input
          type="text"
          name="title"
          value={form.title}
          onChange={(event) => updateField("title", event.target.value)}
          maxLength={200}
          required
          autoFocus
        />
      </label>

      <label>
        Description
        <textarea
          name="description"
          value={form.description}
          onChange={(event) => updateField("description", event.target.value)}
          maxLength={2000}
          rows={3}
        />
      </label>

      <label>
        Status
        <select
          name="status"
          value={form.status}
          onChange={(event) => updateField("status", event.target.value)}
        >
          {TASK_STATUSES.map((status) => (
            <option key={status} value={status}>{status}</option>
          ))}
        </select>
      </label>

      <label>
        Priority
        <select
          name="priority"
          value={form.priority}
          onChange={(event) => updateField("priority", event.target.value)}
        >
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
        </select>
      </label>

      <label>
        Due date
        <input
          type="datetime-local"
          name="dueDate"
          value={form.dueDate}
          onChange={(event) => updateField("dueDate", event.target.value)}
        />
      </label>

      <label>
        Assignee
        <select
          name="assigneeId"
          value={form.assigneeId}
          onChange={(event) => updateField("assigneeId", event.target.value)}
          disabled={assigneesLoading || Boolean(assigneesError)}
        >
          <option value="">
            {assigneesLoading
              ? "Loading project members..."
              : assigneesError
                ? "Members unavailable"
                : "Unassigned"}
          </option>
          {assignees.map((assignee) => (
            <option key={assignee.id} value={assignee.id}>
              {assignee.label}
            </option>
          ))}
        </select>
        {assigneesError && (
          <small className="assignee-error" role="alert">
            Could not load project members: {assigneesError}
          </small>
        )}
      </label>

      <div className="task-form-actions">
        <button type="submit" disabled={saving}>
          {saving ? "Saving..." : "Save task"}
        </button>

        <button type="button" onClick={onCancel}>
          Cancel
        </button>
      </div>
    </form>
  );
}
