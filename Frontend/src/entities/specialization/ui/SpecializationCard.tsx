import { Card, CardContent, Typography, Box } from "@mui/material";
import type { Specialization } from "../model/specialization";

interface SpecializationCardProps {
  specialization: Specialization;
}

export function SpecializationCard({
  specialization,
}: Readonly<SpecializationCardProps>) {
  return (
    <Card variant="outlined" sx={{ height: "100%" }}>
      <CardContent>
        <Box>
          <Typography variant="h6">{specialization.name}</Typography>
          <Typography variant="body2" color="text.secondary">
            {specialization.code ?? "No code"}
          </Typography>
        </Box>
      </CardContent>
    </Card>
  );
}
