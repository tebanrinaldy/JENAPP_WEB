import { Modal, Button } from "react-bootstrap";
import { useState } from "react";
import CategoriaSelector from "./CategoriaSelector";
import VentaForm from "./VentaForm";
import { createSale } from "../api/sales";
import { getcategories } from "../api/categorias";

function VentaModal({ onClose, onConfirm }) {
  const [carrito, setCarrito] = useState([]);
  const [clienteData, setClienteData] = useState(null);

 const handleConfirm = async () => {
  if (!clienteData || carrito.length === 0) return;

  try {
   const nuevaVenta = {
  client: clienteData.client,
  email: clienteData.email,
  phone: clienteData.phone,
  paymentMethod: clienteData.paymentMethod,
  address: clienteData.address,
  total: carrito.reduce(
    (acc, item) => acc + item.cantidad * item.precio,
    0
  ),
  details: carrito.map((item) => ({
    productId: item.id,
    quantity: item.cantidad,
    unitPrice: item.precio,
    totalPrice: item.precio * item.cantidad,
  })),
};


    const saleCreada = await createSale(nuevaVenta);
    onConfirm(saleCreada);
    setCarrito([]);
    setClienteData(null);
    onClose();
  } catch (error) {
    console.error(error);
  }
};

  return (
    <Modal show onHide={onClose} size="lg" centered>
      <Modal.Header closeButton>
        <Modal.Title>Crear nueva venta</Modal.Title>
      </Modal.Header>

      <Modal.Body>
        <CategoriaSelector carrito={carrito} setCarrito={setCarrito} />

        {carrito.length > 0 && (
          <div className="mt-4">
            <h5>Carrito:</h5>
            <ul className="list-group mb-3">
              {carrito.map((item) => (
                <li
                  key={item.id}
                  className="list-group-item d-flex justify-content-between"
                >
                  <span>
                    {item.nombre} x {item.cantidad}
                  </span>
                  <span>${(item.precio * item.cantidad).toLocaleString()}</span>
                </li>
              ))}
              <li className="list-group-item d-flex justify-content-between">
                <strong>Total</strong>
                <strong>
                  $
                  {carrito
                    .reduce((acc, item) => acc + item.precio * item.cantidad, 0)
                    .toLocaleString()}
                </strong>
              </li>
            </ul>
          </div>
        )}

        <VentaForm onConfirm={setClienteData} />
      </Modal.Body>

      <Modal.Footer>
        <Button variant="secondary" onClick={onClose}>
          Cancelar
        </Button>
        <Button
          variant="primary"
          onClick={handleConfirm}
          disabled={!clienteData || carrito.length === 0}
        >
          Confirmar venta
        </Button>
      </Modal.Footer>
    </Modal>
  );
}

export default VentaModal;
