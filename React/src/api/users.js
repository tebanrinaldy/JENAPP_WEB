import { BASE_API_URL } from "./baseurl";

const API_URL = `${BASE_API_URL}/api/Users`;

export async function getusers() {
  try {
    const response = await fetch(API_URL, {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${sessionStorage.getItem("token")}`,
      },
    });

    if (!response.ok) {
      console.error(`Error HTTP: ${response.status}`);
      return [];
    }

    const text = await response.text();
    return text ? JSON.parse(text) : [];
  } catch (error) {
    console.error("Error al obtener usuarios:", error);
    return [];
  }
}

export async function createuser(user) {
  try {
    const response = await fetch(API_URL, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${sessionStorage.getItem("token")}`,
      },
      body: JSON.stringify(user),
    });

    if (!response.ok) {
      throw new Error(`Error al crear usuario: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error(error);
    return null;
  }
}

export const loginUser = async (credentials) => {
  const res = await fetch(`${API_URL}/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      Username: credentials.username,
      Password: credentials.password,
    }),
  });

  if (!res.ok) {
    throw new Error("Usuario o contraseña incorrectos");
  }

  return res.json();
};
