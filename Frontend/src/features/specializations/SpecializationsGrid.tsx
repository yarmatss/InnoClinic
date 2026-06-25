import {
  Alert,
  Box,
  Card,
  CardContent,
  Grid,
  Pagination,
  Stack,
  Typography,
} from "@mui/material";
import type { Specialization } from "./types";

interface SpecializationsGridProps {
  items: Specialization[];
  totalPages: number;
  pageNumber: number;
  onPageChange: (page: number) => void;
}

export function SpecializationsGrid({
  items,
  totalPages,
  pageNumber,
  onPageChange,
}: SpecializationsGridProps) {
  if (items.length === 0) {
    return <Alert severity="info">No specializations found.</Alert>;
  }

  return (
    <Stack spacing={2}>
      <Grid container spacing={2}>
        {items.map((specialization) => (
          <Grid key={specialization.id} size={{ xs: 12, sm: 6, md: 4 }}>
            <Card variant="outlined">
              <CardContent>
                <Box>
                  <Typography variant="h6">{specialization.name}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {specialization.code ?? "No code"}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {totalPages > 1 && (
        <Pagination
          count={totalPages}
          page={pageNumber}
          onChange={(_, value) => {
            onPageChange(value);
          }}
          color="primary"
        />
      )}
    </Stack>
  );
}
