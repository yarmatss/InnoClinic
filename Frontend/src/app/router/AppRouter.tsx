import { lazy, Suspense } from "react";
import { Route, Routes } from "react-router-dom";
import { Box, CircularProgress } from "@mui/material";

const SpecializationsPage = lazy(async () => {
  const m = await import("@pages/specializations");
  return { default: m.SpecializationsPage };
});

const NotFoundPage = lazy(async () => {
  const m = await import("@pages/not-found");
  return { default: m.NotFoundPage };
});

export function AppRouter() {
  return (
    <Suspense
      fallback={
        <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
          <CircularProgress />
        </Box>
      }
    >
      <Routes>
        {/* TODO: Add a true home page variant once implemented */}
        <Route path="/" element={<SpecializationsPage />} />
        <Route path="/specializations" element={<SpecializationsPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Suspense>
  );
}
