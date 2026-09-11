import { Route, Routes } from "react-router-dom";
import { SpecializationsPage } from "@pages/specializations";
import { NotFoundPage } from "@pages/not-found";

export function AppRouter() {
  return (
    <Routes>
      {/* TODO: Add a true home page variant once implemented */}
      <Route path="/" element={<SpecializationsPage />} />
      <Route path="/specializations" element={<SpecializationsPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
