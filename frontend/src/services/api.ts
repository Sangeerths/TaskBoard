const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
const TOKEN_KEY = "token";
let legacyTokenCleanupComplete = false;

function clearLegacyTokenOnce() {
  if (legacyTokenCleanupComplete) return;
  legacyTokenCleanupComplete = true;

  try {
    localStorage.removeItem(TOKEN_KEY);
  } catch {
    // Storage can be unavailable in restricted browser contexts.
  }
}

export const tokenStorage = {
  // Keep bearer credentials only for this tab's lifetime. This reduces
  // persistence compared with localStorage, though it does not prevent XSS.
  get: () => {
    clearLegacyTokenOnce();
    try {
      return sessionStorage.getItem(TOKEN_KEY);
    } catch {
      return null;
    }
  },
  set: (token: string) => {
    try {
      sessionStorage.setItem(TOKEN_KEY, token);
    } catch {
      throw new Error("This browser does not allow a secure sign-in session.");
    }
  },
  clear: () => {
    try {
      sessionStorage.removeItem(TOKEN_KEY);
    } catch {
      // Continue clearing legacy storage where available.
    }
    legacyTokenCleanupComplete = false;
    clearLegacyTokenOnce();
  },
};

export class ApiError extends Error {
  status: number;
  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

// Lets AuthContext react to a 401 (set by AuthContext on mount)
let onUnauthorized: (() => void) | null = null;
export const setUnauthorizedHandler = (fn: () => void) => {
  onUnauthorized = fn;
};
function extractMessage(data: unknown, status: number): string {
  if (typeof data === "string" && data) return data;

  if (typeof data === "object" && data !== null) {
    const d = data as Record<string, unknown>;

    if (typeof d.message === "string") return d.message;

    // ASP.NET validation errors: { errors: { Field: ["msg"] } }
    if (d.errors && typeof d.errors === "object") {
      const first = Object.values(d.errors as Record<string, string[]>)[0];
      if (Array.isArray(first) && first[0]) return first[0];
    }

    if (typeof d.title === "string") return d.title;
  }

  return `Request failed: ${status}`;
}
export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {},
): Promise<T> {
  if (!API_BASE_URL) {
    throw new Error("VITE_API_BASE_URL is not configured.");
  }

  let apiBase: URL;
  try {
    apiBase = new URL(API_BASE_URL, window.location.origin);
  } catch {
    throw new Error("VITE_API_BASE_URL must be a valid URL.");
  }
  const isLocalApi = ["localhost", "127.0.0.1", "::1"].includes(apiBase.hostname);
  if (apiBase.protocol !== "https:" && !(isLocalApi && apiBase.protocol === "http:")) {
    throw new Error("The API must use HTTPS outside local development.");
  }

  if (/^(?:[a-z][a-z\d+.-]*:|\/\/)/i.test(endpoint) || endpoint.split("/").includes("..")) {
    throw new Error("API endpoints must be relative paths.");
  }
  const normalizedBase = `${apiBase.toString().replace(/\/+$/, "")}/`;
  const requestUrl = new URL(endpoint.replace(/^\/+/, ""), normalizedBase);
  if (requestUrl.origin !== apiBase.origin) {
    throw new Error("API endpoint resolved outside the configured API host.");
  }

  const token = tokenStorage.get();

  const headers = new Headers(options.headers);
  if (!headers.has("Accept")) headers.set("Accept", "application/json");
  // GETs and body-less requests do not need a JSON content type. Omitting it
  // also avoids unnecessary CORS preflights for cross-origin API deployments.
  if (options.body != null && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }
  if (token && !headers.has("Authorization")) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  let response: Response;
  try {
    response = await fetch(requestUrl, {
      ...options,
      headers,
      credentials: "omit",
      cache: "no-store",
      // APIs may redirect to a canonical route or authentication endpoint.
      redirect: "follow",
    });
  } catch (error) {
    if (error instanceof Error && error.name === "AbortError") throw error;
    throw new ApiError(
      `Could not reach the API endpoint ${requestUrl.pathname}. Check that the API is available and allows requests from this app (CORS).`,
      0,
    );
  }

  const text = await response.text();

  let data: unknown = null;
  if (text) {
    try {
      data = JSON.parse(text);
    } catch {
      data = text;
    }
  }

  if (!response.ok) {
    // Don't treat a wrong-password login as "session expired"
    if (response.status === 401 && token) {
      tokenStorage.clear();
      onUnauthorized?.();
    }
    throw new ApiError(extractMessage(data, response.status), response.status);
  }

  return data as T;
}

// Small helpers so services stay short
export const api = {
  get: <T>(url: string, options: Omit<RequestInit, "method" | "body"> = {}) =>
    apiRequest<T>(url, { ...options, method: "GET" }),
  post: <T>(url: string, body?: unknown) =>
    apiRequest<T>(url, { method: "POST", body: body == null ? undefined : JSON.stringify(body) }),
  put: <T>(url: string, body?: unknown) =>
    apiRequest<T>(url, { method: "PUT", body: body == null ? undefined : JSON.stringify(body) }),
  delete: <T = void>(url: string) => apiRequest<T>(url, { method: "DELETE" }),
};
