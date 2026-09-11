import { useCallback, useMemo } from "react";
import { useSearchParams } from "react-router-dom";

export function useSpecializationsSearchParams() {
  const [searchParams, setSearchParams] = useSearchParams();

  const nameFilter = useMemo(
    () => searchParams.get("search") ?? "",
    [searchParams],
  );

  const pageNumber = useMemo(() => {
    const page = Number(searchParams.get("page") ?? "1");
    return Number.isInteger(page) && page > 0 ? page : 1;
  }, [searchParams]);

  const pageSize = useMemo(() => {
    const size = Number(searchParams.get("pageSize") ?? "10");
    return [5, 10, 25].includes(size) ? size : 10;
  }, [searchParams]);

  const sortOrder = useMemo<"asc" | "desc">(
    () => (searchParams.get("sort") === "desc" ? "desc" : "asc"),
    [searchParams],
  );

  const handleApplyFilter = useCallback(
    (params: { name: string; pageSize: number; sortOrder: "asc" | "desc" }) => {
      const next = new URLSearchParams();
      if (params.name) {
        next.set("search", params.name);
      }
      if (params.pageSize !== 10) {
        next.set("pageSize", String(params.pageSize));
      }
      if (params.sortOrder !== "asc") {
        next.set("sort", params.sortOrder);
      }
      next.set("page", "1");
      setSearchParams(next);
    },
    [setSearchParams],
  );

  const handleClearFilter = useCallback(() => {
    setSearchParams(new URLSearchParams());
  }, [setSearchParams]);

  const handlePageChange = useCallback(
    (page: number) => {
      const next = new URLSearchParams(searchParams);
      next.set("page", String(page));
      setSearchParams(next);
    },
    [searchParams, setSearchParams],
  );

  return {
    nameFilter,
    pageNumber,
    pageSize,
    sortOrder,
    handleApplyFilter,
    handleClearFilter,
    handlePageChange,
  };
}
