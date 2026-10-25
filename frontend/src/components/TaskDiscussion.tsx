import { useCallback, useEffect, useState } from "react";
import type { FormEvent } from "react";
import { MoreHorizontal, Send } from "lucide-react";
import { api } from "../services/api";
import "./TaskDiscussion.css";

interface TaskDiscussionProps {
  taskId: string;
  onCommentChange: () => void;
}

interface TaskComment {
  commentId: string;
  content: string;
  createdAt?: string;
  userEmail?: string;
}

interface ContentRequest {
  content: string;
}

function getArray(response: unknown): Record<string, unknown>[] {
  let value: unknown = response;

  if (value && typeof value === "object" && !Array.isArray(value)) {
    const result = value as Record<string, unknown>;
    value =
      result.comments ??
      result.activities ??
      result.activity ??
      result.data ??
      result.results ??
      result;
  }

  return Array.isArray(value)
    ? value.filter(
        (item): item is Record<string, unknown> =>
          item !== null && typeof item === "object"
      )
    : [];
}

function getText(
  item: Record<string, unknown>,
  ...keys: string[]
): string {
  for (const key of keys) {
    const value = item[key];
    if (typeof value === "string" && value.length > 0) {
      return value;
    }
  }

  return "";
}

function getTimestamp(item: Record<string, unknown>): string | undefined {
  const value = [
    item.createdAt,
    item.CreatedAt,
    item.timestamp,
    item.Timestamp,
    item.createdOn,
    item.CreatedOn,
    item.postedAt,
    item.PostedAt,
    item.dateCreated,
    item.DateCreated,
  ].find(
    (candidate): candidate is string | number =>
      (typeof candidate === "string" && candidate.length > 0) ||
      typeof candidate === "number",
  );

  if (typeof value === "number") {
    const milliseconds = value < 1e12 ? value * 1000 : value;
    const date = new Date(milliseconds);
    return Number.isNaN(date.getTime()) ? String(value) : date.toISOString();
  }

  return value;
}

function mapComments(response: unknown): TaskComment[] {
  return getArray(response).map((item) => ({
    commentId: getText(item, "commentId", "CommentId", "id", "Id"),
    content: getText(item, "content", "Content"),
    createdAt: getTimestamp(item),
    userEmail: getText(
      item,
      "userEmail",
      "UserEmail",
      "email",
      "Email",
      "authorName",
      "AuthorName"
    ),
  }));
}

function formatDate(value?: string) {
  if (!value) return "";

  const date = new Date(value);

  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleString(undefined, {
        dateStyle: "medium",
        timeStyle: "short",
      });
}

function getRequestError(reason: unknown, fallback: string): string {
  if (reason instanceof TypeError && reason.message === "Failed to fetch") {
    return "The API could not be reached. Check that the backend is running and that its CORS policy allows this frontend.";
  }

  return reason instanceof Error ? reason.message : fallback;
}

export default function TaskDiscussion({
  taskId,
  onCommentChange,
}: TaskDiscussionProps) {
  const [comments, setComments] = useState<TaskComment[]>([]);
  const [content, setContent] = useState("");
  const [editingId, setEditingId] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<TaskComment | null>(null);
  const [actionMenuId, setActionMenuId] = useState<string | null>(null);
  const [editingContent, setEditingContent] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [commentsLoadError, setCommentsLoadError] = useState("");
  const [commentsError, setCommentsError] = useState("");
  const [message, setMessage] = useState("");

  const loadDiscussion = useCallback(async () => {
    setLoading(true);
    setCommentsLoadError("");
    setCommentsError("");

    const commentResult = await api
      .get<unknown>(
        `api/Comments/tasks/${encodeURIComponent(taskId)}/comments`,
      )
      .then(
        (value) => ({ status: "fulfilled" as const, value }),
        (reason: unknown) => ({ status: "rejected" as const, reason }),
      );

    if (commentResult.status === "fulfilled") {
      setComments(mapComments(commentResult.value));
    } else {
      setComments([]);
      setCommentsLoadError(
        getRequestError(commentResult.reason, "Unable to load comments."),
      );
    }

    setLoading(false);
  }, [taskId]);

  useEffect(() => {
    void Promise.resolve().then(() => loadDiscussion());
  }, [loadDiscussion]);

  async function addComment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmedContent = content.trim();
    if (!trimmedContent) return;

    setSaving(true);
    setCommentsError("");
    setMessage("");

    try {
      const request: ContentRequest = { content: trimmedContent };

      await api.post(
        `api/Comments/tasks/${taskId}/comments`,
        request
      );

      setContent("");
      setMessage("Comment added.");
      await loadDiscussion();
      onCommentChange();
    } catch (err) {
      setCommentsError(
        err instanceof Error ? err.message : "Unable to add comment."
      );
    } finally {
      setSaving(false);
    }
  }

  function startEditing(comment: TaskComment) {
    setEditingId(comment.commentId);
    setEditingContent(comment.content);
    setCommentsError("");
    setMessage("");
  }

  async function saveEdit(commentId: string) {
    const trimmedContent = editingContent.trim();
    if (!trimmedContent) {
      setCommentsError("Comment cannot be empty.");
      return;
    }

    setSaving(true);
    setCommentsError("");
    setMessage("");

    try {
      const request: ContentRequest = { content: trimmedContent };

      await api.put(
        `api/Comments/comments/${commentId}`,
        request
      );

      setEditingId(null);
      setEditingContent("");
      setMessage("Comment updated.");
      await loadDiscussion();
      onCommentChange();
    } catch (err) {
      setCommentsError(
        err instanceof Error ? err.message : "Unable to update comment."
      );
    } finally {
      setSaving(false);
    }
  }

  async function deleteComment(comment: TaskComment) {
    setCommentsError("");
    setMessage("");
    setSaving(true);

    try {
      await api.delete(
        `api/Comments/comments/${comment.commentId}`
      );

      setMessage("Comment deleted.");
      setDeleteTarget(null);
      await loadDiscussion();
      onCommentChange();
    } catch (err) {
      setCommentsError(
        err instanceof Error ? err.message : "Unable to delete comment."
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <section className="task-discussion">
      <div className="task-discussion__heading">
        <h3>Comments</h3>
        <span>{comments.length}</span>
      </div>

      {commentsError && (
        <p className="task-discussion__notice task-discussion__notice--error">
          {commentsError}
        </p>
      )}

      {message && (
        <p className="task-discussion__notice task-discussion__notice--success">
          {message}
        </p>
      )}

      <form
        className="task-discussion__form"
        onSubmit={addComment}
      >
        <input
          type="text"
          value={content}
          onChange={(event) => setContent(event.target.value)}
          placeholder="Write a comment..."
          maxLength={2000}
          required
        />

        <button
          type="submit"
          className="task-discussion__send"
          aria-label={saving ? "Sending comment" : "Send comment"}
          title={saving ? "Sending..." : "Send"}
          disabled={saving || !content.trim()}
        >
          <Send size={14} />
        </button>
      </form>

      {loading ? (
        <p className="task-discussion__muted">Loading discussion...</p>
      ) : (
        <div className="task-discussion__comments">
          {commentsLoadError ? (
            <div className="task-discussion__load-error">
              <p className="task-discussion__notice task-discussion__notice--error">
                Could not load comments from{" "}
                <code>
                  GET /api/Comments/tasks/{encodeURIComponent(taskId)}/comments
                </code>
                : {commentsLoadError}
              </p>
              <button
                type="button"
                className="task-discussion__secondary"
                onClick={() => void loadDiscussion()}
              >
                Retry
              </button>
            </div>
          ) : comments.length === 0 ? (
            <p className="task-discussion__muted">
              No comments yet.
            </p>
          ) : (
            comments.map((comment) => (
              <article
                className="task-discussion__comment"
                key={comment.commentId}
              >
                <div className="task-discussion__comment-meta">
                  <strong>{comment.userEmail || "Project member"}</strong>
                  <div className="task-discussion__comment-tools">
                    {comment.createdAt && (
                      <time dateTime={comment.createdAt}>
                        {formatDate(comment.createdAt)}
                      </time>
                    )}
                    {editingId !== comment.commentId && (
                      <div className="task-discussion__menu-wrap">
                        <button
                          type="button"
                          className="task-discussion__menu-trigger"
                          aria-label={`Comment actions for ${comment.userEmail || "Project member"}`}
                          aria-haspopup="menu"
                          aria-expanded={actionMenuId === comment.commentId}
                          onClick={() =>
                            setActionMenuId((current) =>
                              current === comment.commentId
                                ? null
                                : comment.commentId,
                            )
                          }
                        >
                          <MoreHorizontal size={19} />
                        </button>
                        {actionMenuId === comment.commentId && (
                          <div
                            className="task-discussion__menu"
                            role="menu"
                            aria-label="Comment actions"
                          >
                            <button
                              type="button"
                              role="menuitem"
                              onClick={() => {
                                setActionMenuId(null);
                                startEditing(comment);
                              }}
                            >
                              Edit
                            </button>
                            <button
                              type="button"
                              role="menuitem"
                              className="task-discussion__menu-delete"
                              onClick={() => {
                                setActionMenuId(null);
                                setDeleteTarget(comment);
                              }}
                            >
                              Delete
                            </button>
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                </div>

                <p className="task-discussion__content">
                  {comment.content}
                </p>
              </article>
            ))
          )}
        </div>
      )}

      {editingId && (
        <div
          className="discussion-modal-backdrop"
          role="presentation"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget && !saving) {
              setEditingId(null);
              setEditingContent("");
            }
          }}
        >
          <section
            className="discussion-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="edit-comment-title"
          >
            <h3 id="edit-comment-title">Edit comment</h3>
            <textarea
              value={editingContent}
              onChange={(event) => setEditingContent(event.target.value)}
              rows={4}
              maxLength={2000}
              autoFocus
            />
            <div className="discussion-modal__actions">
              <button
                type="button"
                className="discussion-modal__cancel"
                disabled={saving}
                onClick={() => {
                  setEditingId(null);
                  setEditingContent("");
                }}
              >
                Cancel
              </button>
              <button
                type="button"
                disabled={saving || !editingContent.trim()}
                onClick={() => void saveEdit(editingId)}
              >
                {saving ? "Saving..." : "Save"}
              </button>
            </div>
          </section>
        </div>
      )}

      {deleteTarget && (
        <div
          className="discussion-modal-backdrop"
          role="presentation"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget && !saving) {
              setDeleteTarget(null);
            }
          }}
        >
          <section
            className="discussion-modal discussion-modal--confirm"
            role="alertdialog"
            aria-modal="true"
            aria-labelledby="delete-comment-title"
            aria-describedby="delete-comment-description"
          >
            <h3 id="delete-comment-title">Delete comment?</h3>
            <p id="delete-comment-description">
              This comment will be permanently deleted.
            </p>
            <div className="discussion-modal__actions">
              <button
                type="button"
                className="discussion-modal__cancel"
                disabled={saving}
                onClick={() => setDeleteTarget(null)}
              >
                Cancel
              </button>
              <button
                type="button"
                className="discussion-modal__delete"
                disabled={saving}
                onClick={() => void deleteComment(deleteTarget)}
              >
                {saving ? "Deleting..." : "Delete"}
              </button>
            </div>
          </section>
        </div>
      )}
    </section>
  );
}
