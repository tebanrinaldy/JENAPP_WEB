import { Routes, Route, Navigate } from "react-router-dom";
import Dashboard from "./pages/Dashboard.jsx";
import Productos from "./pages/Productos.jsx";
import Venta from "./pages/Venta.jsx";
import Login from "./pages/Login.jsx";
import Register from "./pages/Register.jsx";
import Categorias from "./pages/Categorias.jsx";
import Inventario from "./pages/Inventario.jsx";
import Publico from "./pages/PedidoPublico.jsx";
import PrivateRoute from "./components/PrivateRoute.jsx";
import MainLayout from "./layouts/MainLayout.jsx";
import ReporteVenta from "./pages/ReportesVentas.jsx";
import SeguimientoPedido from "./pages/SeguimientoPedidoPublico.jsx";

function StartRoute() {
  const isLogged = Boolean(localStorage.getItem("user"));
  return <Navigate to={isLogged ? "/dashboard" : "/publico"} replace />;
}

function App() {
  return (
    <Routes>
      
      <Route path="/publico" element={<Publico />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
     <Route path="/seguimiento/:code" element={<SeguimientoPedido />} />
<Route path="/seguimiento/telefono/:phone" element={<SeguimientoPedido />} />

      
      <Route element={<MainLayout />}>
        <Route path="/dashboard" element={<PrivateRoute><Dashboard /></PrivateRoute>} />
        <Route path="/productos" element={<PrivateRoute><Productos /></PrivateRoute>} />
        <Route path="/venta" element={<PrivateRoute><Venta /></PrivateRoute>} />
        <Route path="/categorias" element={<PrivateRoute><Categorias /></PrivateRoute>} />
        <Route path="/inventario" element={<PrivateRoute><Inventario /></PrivateRoute>} />
        <Route path="/reportes-ventas" element={<PrivateRoute><ReporteVenta /></PrivateRoute>} />

      </Route>

      
      <Route path="/" element={<StartRoute />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;