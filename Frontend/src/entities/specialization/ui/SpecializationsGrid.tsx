import { Alert, Grid } from "@mui/material";
import type { Specialization } from "../model/specialization";

import { SpecializationCard } from "./SpecializationCard";

interface SpecializationsGridProps {
  items: Specialization[];
}

export function SpecializationsGrid({
  items,
}: Readonly<SpecializationsGridProps>) {
  if (items.length === 0) {
    return <Alert severity="info">No specializations found.</Alert>;
  }

  return (
    <Grid container spacing={2}>
      {items.map((specialization) => (
        <Grid key={specialization.id} size={{ xs: 12, sm: 6, md: 4 }}>
          <SpecializationCard specialization={specialization} />
        </Grid>
      ))}
    </Grid>
  );
}
