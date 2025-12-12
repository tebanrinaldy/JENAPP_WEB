import React from "react";

function VentaList({ ventas, onVerTicket }) {
  if (!ventas || ventas.length === 0) {
    return <p className="text-muted">No hay ventas registradas.</p>;
  }
  const ventasOrdenadas = [...ventas].sort(
    (a, b) => new Date(b.date) - new Date(a.date)
  );

  return (
    <div className="venta-list">
      {ventasOrdenadas.map((venta) => {
        const fechaFormateada = new Date(venta.date).toLocaleString("es-CO");

        return (
          <div key={venta.id} className="card shadow-sm mb-3">
            <div className="card-body">
              <div className="d-flex justify-content-between align-items-start mb-2">
                <div>
                  <p className="mb-1">
                    <strong>Fecha:</strong> {fechaFormateada}
                  </p>

                  <p className="mb-1">
                    <strong>Cliente:</strong> {venta.client}
                  </p>
                  <p className="mb-1">
                    <strong>Correo:</strong> {venta.email}
                  </p>
                  <p className="mb-1">
                    <strong>Teléfono:</strong> {venta.phone}
                  </p>
                  <p className="mb-1">
                    <strong>Dirección:</strong> {venta.address}
                  </p>
                  <p className="mb-1">
                    <strong>Método de pago:</strong> {venta.paymentMethod}
                  </p>
                  <p className="mb-1">
                    <strong>Total venta:</strong> $
                    {venta.total.toLocaleString()}
                  </p>
                </div>

                {onVerTicket && (
                  <button
                    className="btn btn-sm btn-outline-primary"
                    onClick={() => onVerTicket(venta)}
                  >
                    Ticket PDF
                  </button>
                )}
              </div>

              {venta.details && venta.details.length > 0 && (
                <div className="table-responsive mt-2">
                  <table className="table table-sm mb-0">
                    <thead>
                      <tr>
                        <th>Producto</th>
                        <th>Cantidad</th>
                        <th>Precio</th>
                        <th>Subtotal</th>
                      </tr>
                    </thead>
                    <tbody>
                      {venta.details.map((d) => (
                        <tr key={d.id}>
                          <td>{d.product?.name ?? `Prod ${d.productId}`}</td>
                          <td>{d.quantity}</td>
                          <td>${d.unitPrice.toLocaleString()}</td>
                          <td>${d.totalPrice.toLocaleString()}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}

export default VentaList;
