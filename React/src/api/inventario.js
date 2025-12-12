import { BASE_API_URL } from "./baseurl";

const API_URL = `${BASE_API_URL}/api/inventorymovements`;

export const registrarMovimiento = async (movimiento) => {
  const res = await fetch(API_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
    body: JSON.stringify(movimiento),
  });

  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.message || "Error al registrar movimiento");
  }

  return res.json();
};

export const getMovimientos = async () => {
  const res = await fetch(API_URL, {
    headers: {
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
  });
  return res.json();
};
