import { useAuth0 } from "@auth0/auth0-react";
import { Box, Button, CircularProgress, Typography } from "@mui/material";
import { auth0Config } from "@shared/config";

export function AuthSection() {
  const { isAuthenticated, isLoading, loginWithRedirect, logout, user } =
    useAuth0();

  const handleLogin = () => {
    void loginWithRedirect();
  };

  const handleLogout = () => {
    void logout({
      logoutParams: {
        returnTo: auth0Config.logoutReturnTo,
      },
    });
  };

  if (isLoading) {
    return (
      <Box sx={{ display: "flex", alignItems: "center", minWidth: 60 }}>
        <CircularProgress size={24} color="inherit" />
      </Box>
    );
  }

  if (isAuthenticated) {
    return (
      <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
        <Typography variant="body2" color="text.secondary">
          {user?.name ?? "Signed in"}
        </Typography>
        <Button variant="outlined" onClick={handleLogout}>
          Logout
        </Button>
      </Box>
    );
  }

  return (
    <Button variant="outlined" onClick={handleLogin}>
      Login
    </Button>
  );
}
