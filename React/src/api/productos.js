import { BASE_API_URL } from "./baseurl";

const API_URL = `${BASE_API_URL}/api/products`;

export const getproductos = async () => {
  const res = await fetch(API_URL, {
    headers: {
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
  });

  if (!res.ok) {
    const text = await res.text();
    console.error("❌ Error al obtener productos:", res.status, text);
    throw new Error(text || "Error al obtener productos");
  }

  return res.json();
};

export const createproducto = async (producto) => {
  const res = await fetch(API_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
    body: JSON.stringify(producto),
  });

  if (!res.ok) {
    const text = await res.text();
    console.error("❌ Error al crear producto:", res.status, text);
    throw new Error(text || "Error al crear producto");
  }

  return res.json();
};

export const updateproducto = async (id, producto) => {
  console.log("🔁 Enviando actualización:", id, producto);

  const res = await fetch(`${API_URL}/${id}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
    body: JSON.stringify(producto),
  });

  if (!res.ok) {
    const text = await res.text();
    console.error("❌ Error al actualizar producto:", res.status, text);
    throw new Error(text || "Error al actualizar producto");
  }

  return true;
};

export const deleteproducto = async (id) => {
  const res = await fetch(`${API_URL}/${id}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
  });

  if (!res.ok) {
    const text = await res.text();
    console.error("❌ Error al eliminar producto:", res.status, text);
    throw new Error(text || "Error al eliminar producto");
  }

  return true;
};
