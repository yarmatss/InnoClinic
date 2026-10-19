import { useEffect, useState } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import {
  Alert,
  Box,
  CircularProgress,
  Container,
  Snackbar,
} from "@mui/material";
import { Header } from "@widgets/header";
import { AppRouter } from "./router";

export default function App() {
  const { error, isLoading } = useAuth0();
  const [isDismissed, setIsDismissed] = useState(false);

  useEffect(() => {
    if (error) {
      window.history.replaceState({}, document.title, window.location.pathname);
    }
  }, [error]);

  const handleClose = () => {
    setIsDismissed(true);
  };

  const errorMessage = !isDismissed && error ? error.message : null;

  return (
    <Box sx={{ minHeight: "100vh" }}>
      <Header />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {isLoading ? (
          <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
            <CircularProgress />
          </Box>
        ) : (
          <AppRouter />
        )}
      </Container>
      <Snackbar
        open={Boolean(errorMessage)}
        autoHideDuration={8000}
        onClose={handleClose}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert
          onClose={handleClose}
          severity="error"
          variant="filled"
          sx={{ width: "100%" }}
        >
          {errorMessage}
        </Alert>
      </Snackbar>
    </Box>
  );
}
