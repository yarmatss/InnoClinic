import type { ReactNode } from "react";
import { Auth0Provider } from "@auth0/auth0-react";
import { CssBaseline, ThemeProvider } from "@mui/material";
import { BrowserRouter } from "react-router-dom";
import { auth0Config, appTheme } from "@shared/config";

interface AppProvidersProps {
  children: ReactNode;
}

export function AppProviders({ children }: Readonly<AppProvidersProps>) {
  return (
    <Auth0Provider
      domain={auth0Config.domain ?? ""}
      clientId={auth0Config.clientId ?? ""}
      authorizationParams={auth0Config.authorizationParams}
      cacheLocation="localstorage"
      useRefreshTokens
    >
      <BrowserRouter>
        <ThemeProvider theme={appTheme}>
          <CssBaseline />
          {children}
        </ThemeProvider>
      </BrowserRouter>
    </Auth0Provider>
  );
}
