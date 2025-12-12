import { useEffect, useState } from "react";
import { getproductos } from "../api/productos";
import { getSales } from "../api/sales";
import SalesChart from "../components/SalesChart";
import "../css/Dashboard.css";
import VentasPendientes from "../components/VentasPendientes";

function Dashboard() {
  const [productos, setProductos] = useState([]);
  const [ventas, setVentas] = useState([]);
  const [cargando, setCargando] = useState(true);

  useEffect(() => {
    const cargarDatos = async () => {
      try {
        const prods = await getproductos();
        const sales = await getSales();
        setProductos(prods);
        setVentas(sales);
      } catch (err) {
        console.error("Error cargando datos del dashboard:", err);
      } finally {
        setCargando(false);
      }
    };

    cargarDatos();
  }, []);

  const esHoy = (fecha) => {
    if (!fecha) return false;
    const f = new Date(fecha);
    const hoy = new Date();
    return f.toDateString() === hoy.toDateString();
  };

  const ventasHoy = ventas.filter((v) => esHoy(v.date ?? v.Date));

  const totalVentasHoy = ventasHoy.reduce(
    (acum, v) => acum + (Number(v.total ?? v.Total ?? 0) || 0),
    0
  );

  const cantidadVentasHoy = ventasHoy.length;

  const stockMinimoPorDefecto = 5;
  const productosBajoStock = productos.filter((p) => {
    const stockMinimo = p.stockMin ?? stockMinimoPorDefecto;
    return (p.stock ?? 0) <= stockMinimo;
  }).length;

  if (cargando) return <p className="p-4">Cargando dashboard...</p>;

  return (
    <div className="dashboard-container">
      <div className="container-fluid py-4">
        {/* ENCABEZADO */}
        <div className="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-2">
          <div>
            <h2 className="fw-bold mb-1">Dashboard general</h2>
            <p className="text-muted mb-0">
              Resumen de ventas, comportamiento y stock de productos.
            </p>
          </div>
        </div>

        {/* CARDS RESUMEN */}
        <div className="row g-3 mb-4">
          <div className="col-12 col-md-6 col-lg-3">
            <div className="stat-card stat-card-primary">
              <p className="stat-label">Total ventas de hoy</p>
              <h3 className="stat-value">
                ${totalVentasHoy.toLocaleString("es-CO")}
              </h3>
              <span className="stat-caption">
                Suma de todas las ventas registradas hoy
              </span>
            </div>
          </div>

          <div className="col-12 col-md-6 col-lg-3">
            <div className="stat-card stat-card-secondary">
              <p className="stat-label">Cantidad de ventas hoy</p>
              <h3 className="stat-value">{cantidadVentasHoy}</h3>
              <span className="stat-caption">Número de tickets emitidos</span>
            </div>
          </div>

          <div className="col-12 col-md-6 col-lg-3">
            <div className="stat-card stat-card-neutral">
              <p className="stat-label">Productos registrados</p>
              <h3 className="stat-value">{productos.length}</h3>
              <span className="stat-caption">
                Total de productos en tu inventario
              </span>
            </div>
          </div>

          <div className="col-12 col-md-6 col-lg-3">
            <div className="stat-card stat-card-alert">
              <p className="stat-label">Productos por agotarse</p>
              <h3 className="stat-value">{productosBajoStock}</h3>
              <span className="stat-caption">Stock bajo o en nivel mínimo</span>
            </div>
          </div>
        </div>

        {/* GRÁFICA EN CARD */}
        <div className="card shadow-sm soft-card mb-4">
          <div className="card-body">
            <div className="d-flex justify-content-between align-items-center mb-2">
              <h5 className="card-title mb-0">Ventas por día</h5>
              <span className="text-muted small">
                Historial de ventas recientes
              </span>
            </div>
            <SalesChart ventas={ventas} />
          </div>
        </div>
        <VentasPendientes />

        {/* STOCK DE PRODUCTOS */}
        <div className="d-flex justify-content-between align-items-center mb-2">
          <h5 className="mb-0">Stock disponible por producto</h5>
        </div>

        <div className="row g-3">
          {productos.map((p) => {
            const stockMinimo = p.stockMin ?? stockMinimoPorDefecto;
            const stockActual = p.stock ?? 0;
            const estaBajo = stockActual <= stockMinimo;

            return (
              <div key={p.id} className="col-12 col-sm-6 col-md-4 col-lg-3">
                <div
                  className={
                    "soft-card stock-card h-100 " +
                    (estaBajo ? "stock-card-low" : "")
                  }
                >
                  <div className="d-flex flex-column h-100">
                    <div className="mb-2">
                      <h6 className="mb-1 text-truncate" title={p.name}>
                        {p.name}
                      </h6>
                      <p className="mb-1">
                        Stock disponible:{" "}
                        <strong>{stockActual} unidades</strong>
                      </p>
                      <small className="text-muted">
                        Mínimo recomendado: {stockMinimo}
                      </small>
                    </div>

                    {estaBajo && (
                      <div className="mt-auto">
                        <span className="badge bg-danger-subtle text-danger-emphasis rounded-pill px-3 py-1 small">
                          ⚠ Stock bajo, revisa este producto
                        </span>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            );
          })}

          {productos.length === 0 && (
            <p className="text-muted mt-3">
              No hay productos registrados en el inventario.
            </p>
          )}
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
