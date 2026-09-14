import {
  Alert,
  Box,
  Button,
  Pagination,
  Stack,
  Typography,
} from "@mui/material";
import { SpecializationsFilter } from "@features/filter-specializations";
import {
  SpecializationsGrid,
  useSpecializations,
} from "@entities/specialization";
import { useSpecializationsSearchParams } from "../model/useSpecializationsSearchParams";
import { SpecializationsSkeleton } from "./SpecializationsSkeleton";

export function SpecializationsPage() {
  const {
    nameFilter,
    pageNumber,
    pageSize,
    sortOrder,
    handleApplyFilter,
    handleClearFilter,
    handlePageChange,
  } = useSpecializationsSearchParams();

  const { specializations, isLoading, error, totalCount, totalPages, refetch } =
    useSpecializations({
      pageNumber,
      pageSize,
      nameFilter,
      sortOrder,
    });

  return (
    <Stack spacing={3}>
      <title>Specializations | InnoClinic</title>

      <Box>
        <Typography variant="h4" gutterBottom>
          Specializations
        </Typography>
      </Box>

      <SpecializationsFilter
        name={nameFilter}
        pageSize={pageSize}
        sortOrder={sortOrder}
        onApplyFilter={handleApplyFilter}
        onClearFilter={handleClearFilter}
      />

      {error && (
        <Alert
          severity="error"
          action={
            <Button color="inherit" size="small" onClick={refetch}>
              Retry
            </Button>
          }
        >
          {error}
        </Alert>
      )}

      {isLoading && specializations.length === 0 && (
        <SpecializationsSkeleton count={pageSize} />
      )}

      {!error && (specializations.length > 0 || !isLoading) && (
        <Stack
          spacing={2}
          sx={{
            opacity: isLoading ? 0.6 : 1,
            transition: "opacity 0.2s ease-in-out",
            pointerEvents: isLoading ? "none" : "auto",
          }}
        >
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              minHeight: 24,
            }}
          >
            <Typography variant="body2" color="text.secondary">
              Total Results: {totalCount} (Showing {specializations.length}{" "}
              items)
            </Typography>
            {isLoading && (
              <Typography variant="caption" color="text.secondary">
                Updating...
              </Typography>
            )}
          </Box>

          <SpecializationsGrid items={specializations} />

          {totalPages > 1 && (
            <Box sx={{ display: "flex", justifyContent: "center", pt: 2 }}>
              <Pagination
                count={totalPages}
                page={pageNumber}
                onChange={(_, value) => {
                  handlePageChange(value);
                }}
                color="primary"
              />
            </Box>
          )}
        </Stack>
      )}
    </Stack>
  );
}
