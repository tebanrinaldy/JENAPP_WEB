import { BASE_API_URL } from "./baseurl";

export const getPendingSales = async () => {
  const res = await fetch(`${BASE_API_URL}/api/PendingSales`, {
    headers: {
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
  });
  return res.json();
};

export const confirmPendingSale = async (id) => {
  const res = await fetch(`${BASE_API_URL}/api/PendingSales/confirm/${id}`, {
    method: "PUT",
    headers: {
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
  });
  return res.json();
};

export const rejectPendingSale = async (id) => {
  const res = await fetch(`${BASE_API_URL}/api/PendingSales/reject/${id}`, {
    method: "PUT",
    headers: {
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
  });
  return res.json();
};
