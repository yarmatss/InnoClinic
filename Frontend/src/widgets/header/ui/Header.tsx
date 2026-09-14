import { AppBar, Toolbar, Typography } from "@mui/material";
import { Link as RouterLink } from "react-router-dom";
import { AuthSection } from "@features/auth";

export function Header() {
  return (
    <AppBar position="static" color="default" elevation={0}>
      <Toolbar>
        <Typography
          variant="h6"
          component={RouterLink}
          to="/"
          sx={{
            flexGrow: 1,
            textDecoration: "none",
            color: "inherit",
            cursor: "pointer",
          }}
        >
          InnoClinic
        </Typography>

        <AuthSection />
      </Toolbar>
    </AppBar>
  );
}
