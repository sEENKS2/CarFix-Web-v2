import { createContext, useContext, useState, useEffect } from 'react';
import api from '../api/axiosClient';
import { validSessionToken, hasRole } from '../security/session';

const AuthContext = createContext();
const clearSession = () => {
  sessionStorage.removeItem('token');
  // Elimina sesiones antiguas de la versión que persistía datos en localStorage.
  localStorage.removeItem('token');
  localStorage.removeItem('user');
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let active = true;
    const onExpired = () => { clearSession(); setUser(null); };
    window.addEventListener('carfix:session-expired', onExpired);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    const restore = async () => {
      try {
        if (!validSessionToken(sessionStorage.getItem('token'))) { clearSession(); return; }
        const { data } = await api.get('/Usuarios/me');
        if (active) setUser(data);
      } catch { clearSession(); }
      finally { if (active) setLoading(false); }
    };
    restore();
    return () => { active = false; window.removeEventListener('carfix:session-expired', onExpired); };
  }, []);

  const login = async (nombreUsuario, password) => {
    clearSession();
    setUser(null);
    const { data } = await api.post('/Usuarios/login', { nombreUsuario, password });
    if (!validSessionToken(data.token)) throw new Error('La API no devolvió una sesión válida.');
    sessionStorage.setItem('token', data.token);
    const profile = { id: data.id, nombreUsuario: data.nombreUsuario, email: data.email, roles: data.roles };
    setUser(profile);
    return profile;
  };

  const logout = () => { clearSession(); setUser(null); };
  const tieneRol = (rolesPermitidos) => hasRole(user, rolesPermitidos);

  return <AuthContext.Provider value={{ user, login, logout, loading, tieneRol }}>{children}</AuthContext.Provider>;
};

export const useAuth = () => useContext(AuthContext);
