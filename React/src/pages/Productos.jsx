import { useState, useEffect } from "react";
import MainLayout from "../layouts/MainLayout";
import {
  getproductos,
  createproducto,
  deleteproducto,
  updateproducto,
} from "../api/productos";
import { getcategories } from "../api/categorias";

function Producto() {
  const [productos, setProductos] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [nuevoproducto, setNuevoProducto] = useState({
    nombre: "",
    precio: "",
    stock: "",
    categoriaid: "",
    imagen: "",
  });

  // id del producto que se está editando (null = modo crear)
  const [editando, seteditando] = useState(null);

  useEffect(() => {
    async function cargardatos() {
      try {
        const cats = await getcategories();
        setCategorias(cats);
        const prods = await getproductos();
        setProductos(prods);
      } catch (error) {
        console.error("Error al cargar datos:", error);
      }
    }
    cargardatos();
  }, []);

  const handlechange = (e) => {
    const { name, value } = e.target;
    setNuevoProducto((prev) => ({ ...prev, [name]: value }));
  };
const guardarproducto = async (e) => {
  e.preventDefault();

  const productoDataBase = {
    name: nuevoproducto.nombre,
    price: parseFloat(nuevoproducto.precio),
    categoryId: parseInt(nuevoproducto.categoriaid),
    stock: parseInt(nuevoproducto.stock),
    imageUrl: nuevoproducto.imagen || null,
  };

  // 👇 si estoy editando, mando también el id dentro del JSON
  const productoData =
    editando !== null
      ? { ...productoDataBase, id: editando }
      : productoDataBase;

  try {
    if (editando !== null) {
      await updateproducto(editando, productoData);
      alert("Producto actualizado exitosamente");
    } else {
      await createproducto(productoData);
      alert("Producto creado exitosamente");
    }

    const update = await getproductos();
    setProductos(update);

    setNuevoProducto({
      nombre: "",
      precio: "",
      stock: "",
      categoriaid: "",
      imagen: "",
    });

    seteditando(null);
  } catch (error) {
    console.error("Error al guardar producto:", error);
    alert(error.message || "Error al guardar producto");
  }
};


  const eliminarProducto = async (id) => {
    if (window.confirm("¿Estás seguro de eliminar este producto?")) {
      try {
        await deleteproducto(id);
        setProductos((prev) => prev.filter((p) => p.id !== id));
        alert("Producto eliminado exitosamente");
      } catch (error) {
        console.error("Error al eliminar producto:", error);
        alert("Error al eliminar producto");
      }
    }
  };

  const editar = (producto) => {
    setNuevoProducto({
      nombre: producto.name,
      precio: producto.price,
      stock: producto.stock,
      categoriaid: String(producto.categoryId), 
      imagen: producto.imageUrl || "",
    });
    seteditando(producto.id); // aquí guardamos el id que luego usamos en update
  };

  return (
    <div className="p-4">
      <h2>Administrar productos</h2>

      <form onSubmit={guardarproducto} className="d-flex gap-2 mb-4 flex-wrap">
        <input
          type="text"
          className="form-control"
          name="nombre"
          placeholder="Nombre"
          value={nuevoproducto.nombre}
          onChange={handlechange}
          required
        />

        <input
          type="number"
          className="form-control"
          name="precio"
          placeholder="Precio"
          value={nuevoproducto.precio}
          onChange={handlechange}
          required
        />

        <input
          type="number"
          className="form-control"
          placeholder="Stock"
          name="stock"
          value={nuevoproducto.stock}
          min="0"
          onChange={handlechange}
        />

        <select
          className="form-select"
          name="categoriaid"
          value={nuevoproducto.categoriaid}
          onChange={handlechange}
          required
        >
          <option value="">Selecciona categoría</option>
          {categorias.map((cat) => (
            <option key={cat.id} value={cat.id}>
              {cat.name}
            </option>
          ))}
        </select>

        <input
          type="text"
          className="form-control"
          name="imagen"
          placeholder="URL de imagen"
          value={nuevoproducto.imagen}
          onChange={handlechange}
        />

        {/* ⭐ Cambiamos el texto según si estamos editando o creando */}
        <button type="submit" className="btn btn-success">
          {editando !== null ? "✅ Actualizar" : "➕ Añadir"}
        </button>
      </form>

      {productos.length === 0 ? (
        <p>No hay productos registrados.</p>
      ) : (
        <ul className="list-group">
          {productos.map((p) => (
            <li
              key={p.id}
              className="list-group-item d-flex justify-content-between align-items-center"
            >
              <div className="d-flex align-items-center gap-3">
                {p.imageUrl && (
                  <img
                    src={p.imageUrl}
                    alt={p.name}
                    style={{
                      width: "60px",
                      height: "60px",
                      objectFit: "cover",
                      borderRadius: "6px",
                    }}
                  />
                )}

                <span>
                  {p.name} – ${p.price.toLocaleString()} –{" "}
                  <strong>Stock: {p.stock}</strong>
                </span>
              </div>

              <div>
                <button
                  className="btn-sm btn-primary me-2"
                  onClick={() => editar(p)}
                >
                  ✏️ Editar
                </button>
                <button
                  className="btn btn-sm btn-danger"
                  onClick={() => eliminarProducto(p.id)}
                >
                  🗑️ Eliminar
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default Producto;
