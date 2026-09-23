import { apiClient } from './apiClient';

export interface LoginResponse {
  token: string;
  tokenType: string;
  expiresInSeconds: number;
  email: string;
  role: string;
}

export const authApi = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/Auth/Login', { email, password });
    return response.data;
  }
};
