import { useAsync } from "../../common/hooks/useAsync";
import { getSpecializations } from "./specializationsApi";

interface UseSpecializationsProps {
  pageNumber: number;
  pageSize: number;
  nameFilter: string;
  sortOrder: "asc" | "desc";
}

export function useSpecializations({
  pageNumber,
  pageSize,
  nameFilter,
  sortOrder,
}: UseSpecializationsProps) {
  const { data, isLoading, error } = useAsync(
    (signal) =>
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

  return {
    specializations: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    totalPages: data?.totalPages ?? 0,
    isLoading,
    error,
  };
}
