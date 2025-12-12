
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
  ResponsiveContainer,
} from "recharts";
import "../css/SalesChart.css";


function agruparVentasPorDia(ventas) {
  const mapa = {};

  ventas.forEach((v) => {
    const fecha = new Date(v.date ?? v.Date);
    if (isNaN(fecha)) return;

    const clave = fecha.toISOString().slice(0, 10); // yyyy-mm-dd
    const total = Number(v.total ?? v.Total ?? 0) || 0;

    mapa[clave] = (mapa[clave] || 0) + total;
  });

  return Object.entries(mapa)
    .sort((a, b) => (a[0] > b[0] ? 1 : -1))
    .map(([fecha, totalDia]) => ({
      fecha,
      totalDia,
    }));
}

export default function SalesChart({ ventas }) {
  const datosAgrupados = agruparVentasPorDia(ventas);
  const data = datosAgrupados.slice(-7); //  últimos 7 días

  if (!data.length) {
    return <p className="text-muted mb-0">Aún no hay datos para mostrar.</p>;
  }

  return (
    <div className="sales-chart-wrapper">
      <ResponsiveContainer width="100%" height={260}>
        <BarChart
          data={data}
          barCategoryGap={30}     
          barSize={40}           
          margin={{ top: 10, right: 20, left: 0, bottom: 30 }}
        >
          
          <defs>
            <linearGradient id="ventasGradient" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#2563eb" stopOpacity={0.9} />
              <stop offset="100%" stopColor="#38bdf8" stopOpacity={0.9} />
            </linearGradient>
          </defs>

          <CartesianGrid strokeDasharray="3 3" vertical={false} />

          <XAxis
            dataKey="fecha"
            angle={-30}
            textAnchor="end"
            height={60}
            tick={{ fontSize: 12, fill: "#64748b" }}
          />

          <YAxis
            tick={{ fontSize: 12, fill: "#64748b" }}
            tickFormatter={(v) => v.toLocaleString("es-CO")}
          />

          <Tooltip
            contentStyle={{
              borderRadius: "0.75rem",
              border: "1px solid #e5e7eb",
              boxShadow: "0 10px 30px rgba(15,23,42,0.12)",
              padding: "0.5rem 0.75rem",
            }}
            labelFormatter={(label) => `Fecha: ${label}`}
            formatter={(valor) => [
              `$ ${Number(valor).toLocaleString("es-CO")}`,
              "Total del día",
            ]}
          />

          <Bar
            dataKey="totalDia"
            fill="url(#ventasGradient)"
            radius={[10, 10, 0, 0]} 
          />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}