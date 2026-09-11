import { Button, Box, Typography } from "@mui/material";
import { Link as RouterLink } from "react-router-dom";

export function NotFoundPage() {
  return (
    <Box sx={{ py: 4 }}>
      <title>Page not found | InnoClinic</title>
      <Typography variant="h4" gutterBottom>
        Page not found
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 2 }}>
        The route you opened does not exist.
      </Typography>
      <Button component={RouterLink} to="/" variant="contained">
        Return to Home page
      </Button>
    </Box>
  );
}
