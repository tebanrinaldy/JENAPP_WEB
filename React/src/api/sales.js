import { BASE_API_URL } from "./baseurl";

const API_URL = `${BASE_API_URL}/api/sales`;

export const getSales = async () => {
  const res = await fetch(API_URL, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
  });
  if (!res.ok) throw new Error("Error al obtener las ventas");
  return res.json();
};

export const createSale = async (sale) => {
  const res = await fetch(API_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
    body: JSON.stringify(sale),
  });

  const text = await res.text();

  if (!res.ok) {
    console.error("Error al crear la venta. Respuesta del servidor:");
    console.error(text);
    throw new Error("Error al crear la venta");
  }

  return JSON.parse(text);
};
