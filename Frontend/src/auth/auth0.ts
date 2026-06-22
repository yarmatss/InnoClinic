const redirectUri = `${window.location.origin}/`
const audience = 'https://innocliniic/profiles'
const scope =
  'openid profile email read:patients write:patients read:staff write:staff \
  write:specializations read:appointments write:appointments \
  confirm:appointments read:results write:results'

export const auth0Config = {
  domain: 'yarmatss.eu.auth0.com',
  clientId: 'U15y9gdVndMyH2cOIK8jrHBuOaMBKFYI',
  audience,
  authorizationParams: {
    redirect_uri: redirectUri,
    audience,
    scope,
  },
  logoutReturnTo: redirectUri,
} as const
