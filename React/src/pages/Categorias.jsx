import { useState, useEffect } from "react";
import {
  getcategories,
  createcategories,
  updatecategories,
  deletecategories,
} from "../api/categorias";

function Categorias() {
  const [categorias, setCategorias] = useState([]);
  const [nombre, setNombre] = useState("");
  const [editando, setEditando] = useState(null);

  useEffect(() => {
    cargarCategorias();
  }, []);

  const cargarCategorias = async () => {
    const data = await getcategories();
    setCategorias(data);
  };

  const handleEditar = (categoria) => {
    setEditando(categoria);
    setNombre(categoria.name);
  };

  const handleCancelar = () => {
    setEditando(null);
    setNombre("");
  };
  const handlecreate = async (e) => {
    e.preventDefault();

    if (
      !editando &&
      categorias.some((cat) => cat.name?.toLowerCase() === nombre.toLowerCase())
    ) {
      alert("La categoría ya existe");
      return;
    }

    if (editando) {
      await updatecategories(editando.id, { name: nombre });
      alert("Categoria actualizada con exito");
      setEditando(null);
    } else {
      await createcategories({ name: nombre });
      alert("Categoria creada con exito");
    }

    setNombre("");
    await cargarCategorias();
  };

  const eliminarCategoria = async (id) => {
    const confirmar = window.confirm(
      "¿Estás seguro de que deseas eliminar esta categoría?"
    );
    if (!confirmar) return;
    await deletecategories(id);
    alert("Categoría eliminada con éxito");
    await cargarCategorias();
  };

  return (
    <div className="categoria-page p-4">
      <h2>Categorías de productos</h2>

      <form onSubmit={handlecreate} className="d-flex gap-2 mb-4">
        <input
          type="text"
          className="form-control"
          placeholder="Nueva categoría"
          value={nombre}
          onChange={(e) => setNombre(e.target.value)}
          required
        />

        {editando ? (
          <>
            <button type="submit" className="btn btn-warning">
              Guardar cambios
            </button>
            <button
              type="button"
              className="btn btn-secondary"
              onClick={handleCancelar}
            >
              Cancelar
            </button>
          </>
        ) : (
          <button type="submit" className="btn btn-success">
            ➕ Crear
          </button>
        )}
      </form>

      <ul className="list-group">
        {categorias.map((cat) => (
          <li
            key={cat.id}
            className="list-group-item d-flex justify-content-between align-items-center"
          >
            <span>{cat.name}</span>
            <div className="d-flex gap-2">
              <button
                className="btn btn-sm btn-primary"
                onClick={() => handleEditar(cat)}
              >
                Editar
              </button>
              <button
                className="btn btn-sm btn-danger"
                onClick={() => eliminarCategoria(cat.id)}
              >
                Eliminar
              </button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default Categorias;
