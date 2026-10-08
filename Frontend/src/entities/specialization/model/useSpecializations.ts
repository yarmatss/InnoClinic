import { useCallback } from "react";
import { useAsync } from "@shared/lib/hooks";
import { getSpecializations } from "../api/specializationsApi";

interface UseSpecializationsProps {
  pageNumber: number;
  pageSize: number;
  nameFilter: string;
  sortOrder: "asc" | "desc";
  enabled?: boolean;
}

export function useSpecializations({
  pageNumber,
  pageSize,
  nameFilter,
  sortOrder,
  enabled = true,
}: UseSpecializationsProps) {
  const fetchSpecializations = useCallback(
    (signal: AbortSignal) =>
      getSpecializations({
        pageNumber,
        pageSize,
        name: nameFilter || undefined,
        sortBy: "Name",
        sortOrder: sortOrder,
        signal,
      }),
    [pageNumber, pageSize, nameFilter, sortOrder],
  );

  const { data, isLoading, error, refetch } = useAsync(fetchSpecializations, {
    enabled,
  });

  return {
    specializations: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    totalPages: data?.totalPages ?? 0,
    isLoading,
    error,
    refetch,
  };
}
