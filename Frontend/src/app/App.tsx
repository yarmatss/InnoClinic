import { Box, Container } from "@mui/material";
import { Header } from "@widgets/header";
import { AppRouter } from "./routers";

export default function App() {
  return (
    <Box sx={{ minHeight: "100vh" }}>
      <Header />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <AppRouter />
      </Container>
    </Box>
  );
}
