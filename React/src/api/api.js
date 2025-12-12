
import { BASE_API_URL } from "./baseurl";


export async function apiFetch(endpoint, options = {}) {
  const token = localStorage.getItem("token");

  const headers = {
    "Content-Type": "application/json",
    ...(options.headers || {})
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${BASE_API_URL}${endpoint}`, {
    ...options,
    headers
  });

  return response;
}


export async function apiFetchNoAuth(endpoint, options = {}) {
  const headers = {
    "Content-Type": "application/json",
    ...(options.headers || {})
  };

  const response = await fetch(`${BASE_API_URL}${endpoint}`, {
    ...options,
    headers
  });

  return response;
}

