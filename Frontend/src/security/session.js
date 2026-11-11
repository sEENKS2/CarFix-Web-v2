export function validSessionToken(token, now = Date.now()) {
  if (typeof token !== 'string') return false;
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return false;
    const payload = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
    // Sólo controla caducidad para la interfaz. La firma y los permisos se validan en la API.
    return Number.isFinite(payload.exp) && payload.exp * 1000 > now;
  } catch { return false; }
}

export function hasRole(user, allowed = []) {
  const roles = Array.isArray(user?.roles) ? user.roles : [];
  return roles.includes('Administradores') || allowed.some(role => roles.includes(role));
}
