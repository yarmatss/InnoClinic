import { type ReactNode, useEffect } from "react";
import { Auth0Provider, useAuth0 } from "@auth0/auth0-react";
import { CssBaseline, ThemeProvider } from "@mui/material";
import { BrowserRouter } from "react-router-dom";
import { auth0Config, appTheme } from "@shared/config";
import { setAuthTokenGetter } from "@shared/api";

interface AppProvidersProps {
  children: ReactNode;
}

function AxiosAuthInterceptor({ children }: Readonly<{ children: ReactNode }>) {
  const { getAccessTokenSilently, isAuthenticated } = useAuth0();

  useEffect(() => {
    if (isAuthenticated) {
      setAuthTokenGetter(() =>
        getAccessTokenSilently({
          authorizationParams: {
            audience: auth0Config.audience,
          },
        }),
      );
    } else {
      setAuthTokenGetter(null);
    }
  }, [getAccessTokenSilently, isAuthenticated]);

  return <>{children}</>;
}

function AutoLoginHandler({ children }: Readonly<{ children: ReactNode }>) {
  const { isAuthenticated, isLoading, loginWithRedirect } = useAuth0();

  useEffect(() => {
    if (isLoading || isAuthenticated) {
      return;
    }

    const params = new URLSearchParams(window.location.search);
    const connection = params.get("connection");
    if (connection) {
      const url = new URL(window.location.href);
      url.searchParams.delete("connection");
      window.history.replaceState(
        {},
        document.title,
        url.pathname + (url.search || ""),
      );

      void loginWithRedirect({
        authorizationParams: {
          connection,
        },
      });
    }
  }, [isLoading, isAuthenticated, loginWithRedirect]);

  return <>{children}</>;
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
      <AxiosAuthInterceptor>
        <AutoLoginHandler>
          <BrowserRouter>
            <ThemeProvider theme={appTheme}>
              <CssBaseline />
              {children}
            </ThemeProvider>
          </BrowserRouter>
        </AutoLoginHandler>
      </AxiosAuthInterceptor>
    </Auth0Provider>
  );
}
