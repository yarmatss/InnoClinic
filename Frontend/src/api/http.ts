import axios from "axios";

const baseURL =
  import.meta.env.VITE_PROFILES_API_URL ?? "https://localhost:5001";

export const httpClient = axios.create({
  baseURL,
  headers: {
    "Content-Type": "application/json",
  },
});
