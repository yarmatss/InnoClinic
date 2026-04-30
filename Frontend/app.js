let auth0Client = null;

function getCreateAuth0Client() {
  return window.auth0?.createAuth0Client ?? window.createAuth0Client ?? null;
}

async function initAuth() {
  const createClient = getCreateAuth0Client();
  if (!createClient) {
    throw new Error("Auth0 SPA SDK not loaded. Check the CDN script tag.");
  }

  auth0Client = await createClient({
    domain: config.domain,
    clientId: config.clientId,
    authorizationParams: {
      redirect_uri: config.redirectUri,
      audience: config.audience,
      scope: "openid profile email read:patients write:patients read:staff write:staff write:specializations",
    },
  });

  // Handle the redirect back from Auth0 after login.
  const query = window.location.search;
  if (query.includes("code=") && query.includes("state=")) {
    await auth0Client.handleRedirectCallback();
    // Clean the URL so the ?code= param doesn't stay visible.
    window.history.replaceState({}, document.title, window.location.pathname);
  }

  await updateUI();
}

async function updateUI() {
  const isAuthenticated = await auth0Client.isAuthenticated();

  show("section-unauthenticated", !isAuthenticated);
  show("section-authenticated", isAuthenticated);
  show("section-api", isAuthenticated);

  if (isAuthenticated) {
    const user = await auth0Client.getUser();
    setText("user-name", user.name ?? "—");
    setText("user-email", user.email ?? "—");
    setText("user-sub", user.sub ?? "—");

    const token = await auth0Client.getTokenSilently();
    setText("access-token", token);
    const [, payload] = token.split(".");
    const decoded = JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")));
    setText("token-payload", JSON.stringify(decoded, null, 2));
  } else {
    setText("access-token", "");
    setText("token-payload", "");
    setText("copy-status", "");
  }
}

function show(id, visible) {
  const el = document.getElementById(id);
  if (el) el.style.display = visible ? "block" : "none";
}

function setText(id, value) {
  const el = document.getElementById(id);
  if (el) el.textContent = value;
}

function setStatus(message, isError = false) {
  const el = document.getElementById("api-status");
  el.textContent = message;
  el.className = isError ? "status error" : "status ok";
}

function setResponse(data) {
  document.getElementById("api-response").textContent =
    typeof data === "string" ? data : JSON.stringify(data, null, 2);
}

async function copyAccessToken() {
  const token = document.getElementById("access-token")?.textContent?.trim();
  if (!token) return;

  try {
    if (navigator.clipboard?.writeText) {
      await navigator.clipboard.writeText(token);
    } else {
      const textarea = document.createElement("textarea");
      textarea.value = token;
      document.body.appendChild(textarea);
      textarea.select();
      document.execCommand("copy");
      textarea.remove();
    }

    setText("copy-status", "Copied");
  } catch (err) {
    setText("copy-status", "Copy failed");
  }
}

async function login() {
  if (!auth0Client) await initAuth();
  await auth0Client.loginWithRedirect();
}

async function logout() {
  await auth0Client.logout({
    logoutParams: { returnTo: config.redirectUri },
  });
}

async function callApi() {
  setStatus("Sending request…");
  setResponse("");

  if (!config.apiBaseUrl) {
    setStatus("Set apiBaseUrl in auth_config.js first.", true);
    setResponse("No API base URL configured.");
    return;
  }

  const endpoint = document.getElementById("endpoint-select").value;
  const url = `${config.apiBaseUrl}${endpoint}`;

  try {
    const token = await auth0Client.getTokenSilently();

    const response = await fetch(url, {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    });

    const statusText = `${response.status} ${response.statusText}`;

    if (response.ok) {
      const data = await response.json();
      setStatus(`${statusText}`);
      setResponse(data);
    } else {
      const text = await response.text();
      setStatus(`${statusText}`, true);
      setResponse(text || "(empty body)");
    }
  } catch (err) {
    setStatus(`Network error: ${err.message}`, true);
    setResponse(err.toString());
  }
}

window.addEventListener("load", initAuth);

document.getElementById("btn-login")?.addEventListener("click", login);
document.getElementById("btn-logout")?.addEventListener("click", logout);
document.getElementById("btn-call")?.addEventListener("click", callApi);
document.getElementById("btn-copy-token")?.addEventListener("click", copyAccessToken);
