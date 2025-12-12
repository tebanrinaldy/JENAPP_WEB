import { useState, useEffect } from "react";
import { Button, Card, Row, Col } from "react-bootstrap";
import { getcategories } from "../api/categorias";
import { getproductos } from "../api/productos";
import "../css/CategoriaSelector.css";

function CategoriaSelector({ carrito, setCarrito }) {
  const [categorias, setCategorias] = useState([]);
  const [productos, setProductos] = useState([]);
  const [categoriaSeleccionada, setCategoriaSeleccionada] = useState(null);

  useEffect(() => {
    const cargarDatos = async () => {
      try {
        const cats = await getcategories();
        const prods = await getproductos();
        setCategorias(cats || []);
        setProductos(prods || []);
      } catch (error) {
        console.error("Error al cargar categorías/productos:", error);
      }
    };
    cargarDatos();
  }, []);

  const getImagenUrl = (producto) => {
    const raw = producto.imageUrl || producto.imagen || producto.imgUrl;

    if (!raw) return null;
    return raw;
  };

  const productosFiltrados = categoriaSeleccionada
    ? productos.filter((p) => p.categoryId === Number(categoriaSeleccionada.id))
    : productos;

  const agregarAlCarrito = (producto) => {
    if (producto.stock <= 0) {
      alert("No hay stock disponible para este producto.");
      return;
    }

    const existe = carrito.find((item) => item.id === producto.id);
    if (existe) {
      if (existe.cantidad >= producto.stock) {
        alert("No puedes agregar más unidades, stock insuficiente.");
        return;
      }
      const actualizado = carrito.map((item) =>
        item.id === producto.id
          ? { ...item, cantidad: item.cantidad + 1 }
          : item
      );
      setCarrito(actualizado);
    } else {
      const newItem = {
        id: producto.id,
        nombre: producto.name,
        precio: producto.price,
        cantidad: 1,
        stock: producto.stock,
      };

      const nuevoCarrito = [...carrito, newItem];
      setCarrito(nuevoCarrito);
    }
  };

  return (
    <div>
      <h5>Categorías:</h5>

      <div className="d-flex gap-2 mb-3 flex-wrap">
        {categorias.map((cat) => (
          <button
            key={cat.id}
            className={`btn ${
              categoriaSeleccionada?.id === cat.id
                ? "btn-info"
                : "btn-outline-info"
            }`}
            onClick={() => setCategoriaSeleccionada(cat)}
          >
            {cat.name}
          </button>
        ))}
        <button
          className="btn btn-outline-info"
          onClick={() => setCategoriaSeleccionada(null)}
        >
          Todas
        </button>
      </div>

      <Row>
        {productosFiltrados.length === 0 ? (
          <p>No hay productos disponibles.</p>
        ) : (
          productosFiltrados.map((producto) => {
            const imagenUrl = getImagenUrl(producto);

            return (
              <Col key={producto.id} md={6} lg={4} className="mb-3">
                <Card className="bg-dark text-white product-card h-100">
                  {imagenUrl && (
                    <Card.Img
                      variant="top"
                      src={imagenUrl}
                      alt={producto.name}
                      className="product-card-img"
                    />
                  )}

                  <Card.Body>
                    <Card.Title>{producto.name}</Card.Title>
                    <Card.Text>${producto.price.toLocaleString()}</Card.Text>
                    <Card.Text>Stock disponible: {producto.stock}</Card.Text>
                    <Button
                      variant="info"
                      onClick={() => agregarAlCarrito(producto)}
                      disabled={producto.stock <= 0}
                    >
                      {producto.stock <= 0 ? "Sin stock" : "Añadir"}
                    </Button>
                  </Card.Body>
                </Card>
              </Col>
            );
          })
        )}
      </Row>
    </div>
  );
}

export default CategoriaSelector;
