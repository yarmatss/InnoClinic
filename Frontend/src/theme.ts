import { createTheme } from "@mui/material/styles";

export const appTheme = createTheme({
  palette: {
    mode: "dark",
    primary: {
      main: "#2dd4bf",
    },
    secondary: {
      main: "#8b5cf6",
    },
    background: {
      default: "#050816",
      paper: "#0f172a",
    },
    text: {
      primary: "#f8fafc",
      secondary: "#94a3b8",
    },
  },
  shape: {
    borderRadius: 8,
  },
  typography: {
    fontFamily: [
      "Inter",
      "ui-sans-serif",
      "system-ui",
      "-apple-system",
      "BlinkMacSystemFont",
      "Segoe UI",
      "sans-serif",
    ].join(","),
  },
});
