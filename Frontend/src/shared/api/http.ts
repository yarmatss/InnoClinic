import axios from "axios";

const baseURL =
  (import.meta.env.VITE_PROFILES_API_URL as string | undefined) ??
  "https://localhost:5001";

export const httpClient = axios.create({
  baseURL,
  headers: {
    "Content-Type": "application/json",
  },
});

let tokenGetter: (() => Promise<string>) | null = null;

export const setAuthTokenGetter = (getter: (() => Promise<string>) | null) => {
  tokenGetter = getter;
};

httpClient.interceptors.request.use(async (config) => {
  if (tokenGetter) {
    try {
      const token = await tokenGetter();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    } catch {
      // Proceed without token if acquisition fails
    }
  }
  return config;
});
