import { Grid, Skeleton } from "@mui/material";

interface SpecializationsSkeletonProps {
  count?: number;
}

export function SpecializationsSkeleton({
  count = 10,
}: Readonly<SpecializationsSkeletonProps>) {
  return (
    <Grid container spacing={2}>
      {Array.from({ length: count }).map((_, index) => (
        <Grid
          key={`specialization-skeleton-${String(index)}`}
          size={{ xs: 12, sm: 6, md: 4 }}
        >
          <Skeleton variant="rounded" height={96} sx={{ borderRadius: 1 }} />
        </Grid>
      ))}
    </Grid>
  );
}
