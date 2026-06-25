const redirectUri = `${window.location.origin}/`;
const audience = import.meta.env.VITE_AUTH0_AUDIENCE;
const scope =
  "openid profile email read:patients write:patients read:staff write:staff \
  write:specializations read:appointments write:appointments \
  confirm:appointments read:results write:results";

export const auth0Config = {
  domain: import.meta.env.VITE_AUTH0_DOMAIN,
  clientId: import.meta.env.VITE_AUTH0_CLIENT_ID,
  audience,
  authorizationParams: {
    redirect_uri: redirectUri,
    audience,
    scope,
  },
  logoutReturnTo: redirectUri,
} as const;
