import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { Toaster } from 'sonner';
import Login from './pages/Login';
import MainLayout from './components/MainLayout';
import Dashboard from './pages/Dashboard';
import Tickets from './pages/Tickets';
import Productos from './pages/Productos';
import Proveedores from './pages/Proveedores';
import OrdenesCompra from './pages/OrdenesCompra';
import Auditoria from './pages/Auditoria';
import ClientesVehiculos from './pages/ClientesVehiculos';
import SeguridadUsuarios from './pages/SeguridadUsuarios';
import Tecnicos from './pages/Tecnicos';
import Facturacion from './pages/Facturacion';
import Inventario from './pages/Inventario';
import Turnos from './pages/Turnos';
import CajaDiaria from './pages/CajaDiaria';

function RutaPrivada({ children }) {
  const { user, loading } = useAuth();
  if (loading) return <p role="status">Validando sesión…</p>;
  return user ? children : <Navigate to="/login" replace />;
}

function RutaRol({ roles = [], children }) {
  const { user, tieneRol } = useAuth();
  if (tieneRol(roles)) return children;
  if (user?.roles?.includes('Tecnicos')) return <Navigate to="/tickets" replace />;
  return <p role="alert">No tenés permisos para acceder a esta sección.</p>;
}

export default function App() {
  return (
    <AuthProvider>
      <Toaster position="top-right" richColors closeButton />
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />

          <Route
            path="/"
            element={
              <RutaPrivada>
                <MainLayout />
              </RutaPrivada>
            }
          >
            <Route index element={<RutaRol roles={["Operadores"]}><Dashboard /></RutaRol>} />
            <Route path="dashboard" element={<RutaRol roles={["Operadores"]}><Dashboard /></RutaRol>} />
            <Route path="tickets" element={<RutaRol roles={["Operadores", "Tecnicos"]}><Tickets /></RutaRol>} />
            <Route path="productos" element={<RutaRol roles={["Operadores", "Tecnicos"]}><Productos /></RutaRol>} />
            <Route path="proveedores" element={<RutaRol roles={["Operadores"]}><Proveedores /></RutaRol>} />
            <Route path="ordenes-compra" element={<RutaRol roles={["Operadores"]}><OrdenesCompra /></RutaRol>} />
            <Route path="auditoria" element={<RutaRol roles={[]}><Auditoria /></RutaRol>} />
            <Route path="tecnicos" element={<RutaRol roles={["Operadores"]}><Tecnicos /></RutaRol>} />
            <Route path="clientes-vehiculos" element={<RutaRol roles={["Operadores"]}><ClientesVehiculos /></RutaRol>} />
            <Route path="usuarios" element={<RutaRol roles={[]}><SeguridadUsuarios /></RutaRol>} />
            <Route path="facturacion" element={<RutaRol roles={["Operadores"]}><Facturacion /></RutaRol>} />
            <Route path="inventario" element={<RutaRol roles={["Operadores"]}><Inventario /></RutaRol>} />
            <Route path="turnos" element={<RutaRol roles={["Operadores"]}><Turnos /></RutaRol>} />
            <Route path="caja" element={<RutaRol roles={["Operadores"]}><CajaDiaria /></RutaRol>} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}