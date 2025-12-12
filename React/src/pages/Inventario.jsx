import { useState, useEffect } from "react";
import { getproductos } from "../api/productos";
import { registrarMovimiento, getMovimientos } from "../api/inventario";

function Inventario() {
  const [productos, setProductos] = useState([]);
  const [movimientos, setMovimientos] = useState([]);

  const [productoId, setProductoId] = useState("");
  const [tipo, setTipo] = useState("Entrada");
  const [cantidad, setCantidad] = useState("");
  const [razon, setRazon] = useState("");
  const [fechaFiltro, setFechaFiltro] = useState("");

  const cargarDatos = async () => {
    try {
      const prods = await getproductos();
      const movs = await getMovimientos();
      setProductos(prods);
      setMovimientos(movs);
    } catch (err) {
      console.error("Error al cargar el inventario:", err);
    }
  };
  useEffect(() => {
    cargarDatos();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();

    const cantNum = parseInt(cantidad);

    if (!productoId || cantNum <= 0) {
      alert("Por favor, completa todos los campos correctamente.");
      return;
    }
    const movimiento = {
      productId: parseInt(productoId),
      type: tipo,
      quantity: cantNum,
      reason: razon,
    };

    try {
      await registrarMovimiento(movimiento);
      alert("Movimiento registrado exitosamente");
      setCantidad("");
      setRazon("");
      cargarDatos();
    } catch (error) {
      alert(error.message);
    }
  };
  const movimientosFiltrados = fechaFiltro
    ? movimientos.filter((m) => {
        const fechaMovimiento = new Date(m.date).toISOString().slice(0, 10); // yyyy-mm-dd
        return fechaMovimiento === fechaFiltro;
      })
    : movimientos;

  return (
    <div className="p-4">
      <h2>Gestión de Inventario</h2>

      <form className="d-flex gap-2 flex-wrap mb-4" onSubmit={handleSubmit}>
        <select
          className="form-select"
          value={productoId}
          onChange={(e) => setProductoId(e.target.value)}
          required
        >
          <option value="">Selecciona un producto</option>
          {productos.map((p) => (
            <option key={p.id} value={p.id}>
              {p.name}
            </option>
          ))}
        </select>

        <select
          className="form-select"
          value={tipo}
          onChange={(e) => setTipo(e.target.value)}
        >
          <option value="Entrada">Entrada</option>
          <option value="Salida">Salida</option>
        </select>

        <input
          type="number"
          className="form-control"
          placeholder="Cantidad"
          value={cantidad}
          onChange={(e) => setCantidad(e.target.value)}
          required
        />

        <input
          type="text"
          className="form-control"
          placeholder="Razón (opcional)"
          value={razon}
          onChange={(e) => setRazon(e.target.value)}
        />

        <button className="btn btn-primary">Registrar</button>
      </form>

      <h4>Inventario actual</h4>
      <ul className="list-group mb-4">
        {productos.map((p) => (
          <li
            key={p.id}
            className="list-group-item d-flex justify-content-between"
          >
            <span>{p.name}</span>
            <strong>{p.stock} unidades</strong>
          </li>
        ))}
      </ul>
      <div className="mb-3">
        <label className="form-label">Filtrar por fecha:</label>
        <input
          type="date"
          className="form-control"
          value={fechaFiltro}
          onChange={(e) => setFechaFiltro(e.target.value)}
        />
      </div>

      <h4>Historial de movimientos</h4>
      <ul className="list-group">
        {movimientosFiltrados.map((m) => (
          <li key={m.id} className="list-group-item">
            <strong>{m.product?.name}</strong>
            {" — "}
            {m.type}
            {" — "}
            {m.quantity} {" unidades"}
            <br />
            <small>
              {new Date(m.date).toLocaleString()} — {m.reason || "Sin razon"}
            </small>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default Inventario;
