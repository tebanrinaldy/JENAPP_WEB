import { BASE_API_URL } from "./baseurl";

const API_URL = `${BASE_API_URL}/api/reports`;

export const getSalesReport = async (from, to) => {
  const params = new URLSearchParams();
  params.append("from", from);
  if (to) params.append("to", to);

  const res = await fetch(`${API_URL}/sales-report?${params}`, {
    headers: {
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
  });

  if (!res.ok) throw new Error("Error al obtener el reporte");

  return res.json();
};

export const exportSalesReportPdf = async (from, to) => {
  const params = new URLSearchParams({ from, to });

  const res = await fetch(`${API_URL}/sales-report/pdf?${params}`, {
    headers: {
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
  });

  if (!res.ok) {
    alert("Error generando PDF");
    return;
  }

  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  window.open(url, "_blank");
};

export const exportSaleTicketPdf = async (saleId) => {
  const res = await fetch(`${API_URL}/ticket/${saleId}`, {
    headers: {
      Authorization: `Bearer ${sessionStorage.getItem("token")}`,
    },
  });

  if (!res.ok) {
    alert("Error generando Ticket PDF");
    return;
  }

  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  window.open(url, "_blank");
};
