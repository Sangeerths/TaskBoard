import { api } from "./api";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  statusCode: number;
  message: string;
  token?: string;
  accessToken?: string;
}
export interface CurrentUser {
  id?: number | string;
  email?: string;
  name?: string;
  [key: string]: unknown;
}

export const authService = {
  login: (request: LoginRequest) =>
    api.post<LoginResponse>("api/Auth/login", request),

  me: () => api.get<CurrentUser>("api/Auth/me"),
};
