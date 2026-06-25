import { useState } from "react";
import { Alert, Box, CircularProgress, Stack, Typography } from "@mui/material";
import { useSpecializations } from "./useSpecializations";
import { SpecializationsFilter } from "./SpecializationsFilter";
import { SpecializationsGrid } from "./SpecializationsGrid";

export function SpecializationsPage() {
  const [nameFilter, setNameFilter] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [sortOrder, setSortOrder] = useState<"asc" | "desc">("asc");

  const { specializations, isLoading, error, totalCount, totalPages } =
    useSpecializations({
      pageNumber,
      pageSize,
      nameFilter,
      sortOrder,
    });

  const handleApplyFilter = (params: {
    name: string;
    pageSize: number;
    sortOrder: "asc" | "desc";
  }) => {
    setPageNumber(1);
    setNameFilter(params.name);
    setPageSize(params.pageSize);
    setSortOrder(params.sortOrder);
  };

  const handleClearFilter = () => {
    setPageNumber(1);
    setPageSize(10);
    setNameFilter("");
    setSortOrder("asc");
  };

  return (
    <Stack spacing={3}>
      <title>Specializations | InnoClinic</title>

      <Box>
        <Typography variant="h4" gutterBottom>
          Specializations
        </Typography>
      </Box>

      <SpecializationsFilter
        pageSize={pageSize}
        sortOrder={sortOrder}
        onApplyFilter={handleApplyFilter}
        onClearFilter={handleClearFilter}
      />

      {isLoading && <CircularProgress />}

      {error && <Alert severity="error">{error}</Alert>}

      {!isLoading && !error && (
        <Stack spacing={2}>
          <Typography variant="body2" color="text.secondary">
            Total Results: {totalCount} (Showing {specializations.length} items)
          </Typography>

          <SpecializationsGrid
            items={specializations}
            totalPages={totalPages}
            pageNumber={pageNumber}
            onPageChange={setPageNumber}
          />
        </Stack>
      )}
    </Stack>
  );
}
