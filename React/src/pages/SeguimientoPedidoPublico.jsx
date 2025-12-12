import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { BASE_API_URL } from "../api/baseurl";
import "../css/PedidoPublico.css";

function SeguimientoPedidoPublico() {
  const { code, phone } = useParams();
  const [data, setData] = useState(null);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchPedido = async () => {
      try {
        setCargando(true);
        setError(null);

        const url = phone
          ? `${BASE_API_URL}/api/PendingSales/search?phone=${encodeURIComponent(
              phone
            )}`
          : `${BASE_API_URL}/api/PendingSales/track/${encodeURIComponent(
              code
            )}`;

        const res = await fetch(url);

        if (!res.ok) {
          if (res.status === 404) {
            setError("No se encontró un pedido con esos datos.");
          } else {
            setError("Error al consultar el pedido.");
          }
          setCargando(false);
          return;
        }

        const json = await res.json();
        setData(json);
      } catch (err) {
        console.error(err);
        setError("Error de conexión con el servidor.");
      } finally {
        setCargando(false);
      }
    };

    if (code || phone) {
      fetchPedido();
    }
  }, [code, phone]);

  const renderEstado = (status) => {
    const pasos = ["Pendiente", "Confirmada", "Rechazada"];

    return (
      <div className="seguimiento-steps">
        {pasos.map((paso) => {
          const activo = paso === status;
          return (
            <div
              key={paso}
              className={`step-item ${activo ? "step-item-active" : ""}`}
            >
              <div className="step-circle" />
              <span className="step-label">{paso}</span>
            </div>
          );
        })}
      </div>
    );
  };

  return (
    <div className="pedido-wrapper">
      <div className="pedido-container container-lg">
        <header className="pedido-header d-flex justify-content-between align-items-center">
          <div>
            <h1 className="pedido-title">📦 Seguimiento de pedido</h1>
            <p className="pedido-subtitle">
              Consulta el estado de tu pedido usando tu código o teléfono.
            </p>
          </div>

          <div className="header-actions">
            <Link to="/pedido-publico" className="btn btn-outline-primary">
              🛍 Hacer otro pedido
            </Link>
          </div>
        </header>

        <div className="panel card-shadow mt-3">
          <h2 className="panel-title">
            🔎 Buscando por:{" "}
            {code ? `código ${code}` : `teléfono ${phone}`}
          </h2>

          {cargando && <p>Cargando información...</p>}

          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {!cargando && !error && data && (
            <>
              <div className="mb-3">
                <p className="mb-1">
                  <strong>Cliente:</strong> {data.client}
                </p>
                {data.phone && (
                  <p className="mb-1">
                    <strong>Teléfono:</strong> {data.phone}
                  </p>
                )}
                <p className="mb-1">
                  <strong>Total:</strong> $
                  {data.total?.toLocaleString()}
                </p>
                <p className="mb-1">
                  <strong>Fecha:</strong>{" "}
                  {new Date(data.date).toLocaleString("es-CO")}
                </p>
                <p className="mb-1">
                  <strong>Estado actual:</strong> {data.status}
                </p>
              </div>

              {renderEstado(data.status)}

              <p className="text-muted small mt-3">
                Si tu pedido fue <strong>confirmado</strong>, pronto un
                administrador se pondrá en contacto para coordinar el envío o
                entrega.
              </p>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default SeguimientoPedidoPublico;
