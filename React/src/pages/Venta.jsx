import { useState, useEffect } from "react";
import VentaModal from "../components/VentaModal";
import VentaList from "../components/VentaList";
import { getSales } from "../api/sales";
import { exportSaleTicketPdf } from "../api/reports";
import "../css/Venta.css";

function Venta() {
  const [ventas, setVentas] = useState([]);
  const [mostrarModal, setMostrarModal] = useState(false);
  const [fechaFiltro, setFechaFiltro] = useState("");

  useEffect(() => {
    const cargarVentas = async () => {
      try {
        const data = await getSales();
        setVentas(data);
      } catch (error) {
        console.error("Error al obtener las ventas:", error);
      }
    };
    cargarVentas();
  }, []);

  const actualizarVentas = async () => {
    const data = await getSales();
    setVentas(data);
  };

  const ventasFiltradas = fechaFiltro
    ? ventas.filter((v) => {
        const fechaVenta = new Date(v.date ?? v.Date)
          .toISOString()
          .slice(0, 10);
        return fechaVenta === fechaFiltro;
      })
    : ventas;

  const manejarVerTicket = async (venta) => {
    if (!venta?.id) return;
    try {
      await exportSaleTicketPdf(venta.id);
    } catch (err) {
      console.error(err);
      alert("Error al generar el ticket");
    }
  };

  return (
    <div className="venta-page">
      <div className="container-fluid py-4">
        <div className="d-flex flex-wrap justify-content-between align-items-center mb-4">
          <div>
            <h2 className="fw-bold mb-1">Ventas realizadas</h2>
            <p className="text-muted mb-0">
              Consulta, filtra y administra tus ventas.
            </p>
          </div>

          <button
            className="btn btn-success btn-lg shadow-sm"
            onClick={() => setMostrarModal(true)}
          >
            + Añadir venta
          </button>
        </div>

        <div className="card filtro-card shadow-sm mb-4">
          <div className="card-body d-flex flex-wrap align-items-end gap-3">
            <div>
              <label className="form-label mb-1">Filtrar por fecha</label>
              <input
                type="date"
                className="form-control"
                value={fechaFiltro}
                onChange={(e) => setFechaFiltro(e.target.value)}
              />
            </div>

            {fechaFiltro && (
              <button
                className="btn btn-outline-secondary"
                onClick={() => setFechaFiltro("")}
              >
                Limpiar filtro
              </button>
            )}
          </div>
        </div>

        <VentaList
          ventas={ventasFiltradas}
          onVerTicket={manejarVerTicket}
        />

        {mostrarModal && (
          <VentaModal
            onClose={() => setMostrarModal(false)}
            onConfirm={actualizarVentas}
          />
        )}
      </div>
    </div>
  );
}

export default Venta;
