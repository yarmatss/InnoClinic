import { useAuth0 } from '@auth0/auth0-react'
import { AppBar, Box, Button, Container, Toolbar, Typography } from '@mui/material'
import { Link as RouterLink, Route, Routes, useNavigate } from 'react-router-dom'
import { auth0Config } from './auth/auth0'
import { SpecializationsPage } from './features/specializations/SpecializationsPage'
import { NotFoundPage } from './pages/NotFoundPage'

function App() {
  const { isAuthenticated, isLoading, loginWithRedirect, logout, user } =
    useAuth0()
  const navigate = useNavigate()

  const handleLogin = async () => {
    await loginWithRedirect()
  }

  const handleLogout = async () => {
    await logout({
      logoutParams: {
        returnTo: auth0Config.logoutReturnTo,
      },
    })
    navigate('/')
  }

  return (
    <Box sx={{ minHeight: '100vh' }}>
      <AppBar position="static" color="default" elevation={0}>
        <Toolbar>
          <Typography
            variant="h6"
            component={RouterLink}
            to="/"
            sx={{ flexGrow: 1, textDecoration: 'none', color: 'inherit', cursor: 'pointer' }}
          >
            InnoClinic
          </Typography>
          {isAuthenticated ? (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Typography variant="body2" color="text.secondary">
                {isLoading ? 'Checking session...' : user?.name ?? 'Signed in'}
              </Typography>
              <Button variant="outlined" onClick={handleLogout}>
                Logout
              </Button>
            </Box>
          ) : (
            <Button variant="outlined" onClick={handleLogin}>
              Login
            </Button>
          )}
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Routes>
          <Route path="/" element={<SpecializationsPage />} />
          <Route path="/specializations" element={<SpecializationsPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </Container>
    </Box>
  )
}

export default App
