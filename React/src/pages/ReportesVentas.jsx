import { useState } from "react";
import Flatpickr from "react-flatpickr";
import "flatpickr/dist/themes/material_blue.css";
import { Spanish } from "flatpickr/dist/l10n/es.js";
import {
  getSalesReport,
  exportSalesReportPdf,
  exportSaleTicketPdf,
} from "../api/reports";

function ReportesVentas() {
  const [modo, setModo] = useState("dia");
  const [fechaDia, setFechaDia] = useState(null);
  const [rango, setRango] = useState([]);
  const [reporte, setReporte] = useState(null);

  const obtenerFechas = () => {
    if (modo === "dia") {
      if (!fechaDia) return null;

      const d = Array.isArray(fechaDia) ? fechaDia[0] : fechaDia;
      const fecha = d.toISOString().split("T")[0];
      return { from: fecha, to: fecha };
    }

    if (rango.length !== 2) return null;

    return {
      from: rango[0].toISOString().split("T")[0],
      to: rango[1].toISOString().split("T")[0],
    };
  };

  const buscar = async () => {
    const fechas = obtenerFechas();
    if (!fechas) return alert("Seleccione fechas válidas");

    try {
      const data = await getSalesReport(fechas.from, fechas.to);
      setReporte(data);
    } catch (err) {
      alert("Error al cargar el reporte");
    }
  };

  const exportarPdf = () => {
    const fechas = obtenerFechas();
    if (!fechas) return alert("Seleccione fechas válidas");

    exportSalesReportPdf(fechas.from, fechas.to);
  };

  return (
    <div className="p-3">
      <div className="mb-3 flex gap-2">
        <button
          onClick={() => setModo("dia")}
          className={
            modo === "dia" ? "btn btn-primary" : "btn btn-outline-primary"
          }
        >
          Por día
        </button>

        <button
          onClick={() => setModo("rango")}
          className={
            modo === "rango" ? "btn btn-primary" : "btn btn-outline-primary"
          }
        >
          Por rango
        </button>
      </div>

      {modo === "dia" ? (
        <Flatpickr
          options={{ locale: Spanish, dateFormat: "Y-m-d" }}
          value={fechaDia || ""}
          onChange={(dates) => setFechaDia(dates[0] || null)}
          className="form-control"
        />
      ) : (
        <Flatpickr
          options={{ locale: Spanish, mode: "range", dateFormat: "Y-m-d" }}
          value={rango}
          onChange={(dates) => setRango(dates)}
          className="form-control"
        />
      )}

      <div className="mt-3 d-flex gap-2">
        <button className="btn btn-success" onClick={buscar}>
          Buscar
        </button>

        <button className="btn btn-secondary" onClick={exportarPdf}>
          Exportar PDF
        </button>
      </div>

      {reporte && (
        <div className="mt-4">
          <h4>
            Resultados: {new Date(reporte.desde).toLocaleDateString()} -{" "}
            {new Date(reporte.hasta).toLocaleDateString()}
          </h4>

          <p>
            Total vendido:{" "}
            <strong>${reporte.totalGeneral.toLocaleString()}</strong>
          </p>
          <p>
            Cantidad ventas: <strong>{reporte.cantidadVentas}</strong>
          </p>
          <p>
            Promedio por venta:{" "}
            <strong>${reporte.promedioVentas.toLocaleString()}</strong>
          </p>

          <hr />

          {reporte.ventas.map((v) => (
            <div key={v.id} className="mb-4 p-3 border rounded">
              <div className="d-flex justify-content-between">
                <h5>Venta #{v.id}</h5>
                <button
                  className="btn btn-sm btn-outline-dark"
                  onClick={() => exportSaleTicketPdf(v.id)}
                >
                  Ticket PDF
                </button>
              </div>

              <p>
                Cliente: <strong>{v.cliente}</strong>
              </p>
              <p>
                Fecha: <strong>{new Date(v.fecha).toLocaleString()}</strong>
              </p>
              <p>
                Total venta: <strong>${v.totalVenta.toLocaleString()}</strong>
              </p>

              <table className="table table-sm table-striped mt-3">
                <thead>
                  <tr>
                    <th>Producto</th>
                    <th>Cant.</th>
                    <th>P. Unit</th>
                    <th>Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  {v.detalles.map((d, i) => (
                    <tr key={i}>
                      <td>{d.producto}</td>
                      <td>{d.cantidad}</td>
                      <td>${d.precioUnit.toLocaleString()}</td>
                      <td>${d.subtotal.toLocaleString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default ReportesVentas;
