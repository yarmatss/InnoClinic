const redirectUri = `${globalThis.location.origin}/`;
const audience = import.meta.env.VITE_AUTH0_AUDIENCE as string | undefined;

const scope = "openid profile email offline_access";

export const auth0Config = {
  domain: import.meta.env.VITE_AUTH0_DOMAIN as string | undefined,
  clientId: import.meta.env.VITE_AUTH0_CLIENT_ID as string | undefined,
  audience,
  authorizationParams: {
    redirect_uri: redirectUri,
    audience,
    scope,
  },
  logoutReturnTo: redirectUri,
} as const;
