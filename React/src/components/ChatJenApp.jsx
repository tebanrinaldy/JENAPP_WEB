import { useState } from "react";
import { apiFetchNoAuth } from "../api/api";
import "../css/ChatJenApp.css";

export default function ChatJenApp() {
  const [mensajes, setMensajes] = useState([]);
  const [input, setInput] = useState("");
  const [cargando, setCargando] = useState(false);
  const [visible, setVisible] = useState(false);

  const enviar = async () => {
    if (!input.trim() || cargando) return;

    const texto = input.trim();
    setInput("");

    setMensajes((prev) => [...prev, { de: "usuario", texto }]);

    try {
      setCargando(true);

      const res = await apiFetchNoAuth("/api/chat", {
        method: "POST",
        body: JSON.stringify({ mensaje: texto }),
      });

      const data = await res.json();

      setMensajes((prev) => [
        ...prev,
        { de: "bot", texto: data.respuesta || "No puedo responderte en este momento 😕" },
      ]);
    } catch (err) {
      console.error(err);
      setMensajes((prev) => [
        ...prev,
        { de: "bot", texto: "Error al hablar con el asistente 😓" },
      ]);
    } finally {
      setCargando(false);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      enviar();
    }
  };

  if (!visible) {
    return (
      <button
        className="chat-btn-float"
        onClick={() => setVisible(true)}
      >
        <span className="chat-btn-float-icon" />
        JenBot
      </button>
    );
  }

  return (
    <div className="chat-jenapp">
      <div className="chat-jenapp-header">
        <div className="chat-jenapp-title">
          <div className="chat-jenapp-avatar">J</div>
          <div>
            <div className="chat-jenapp-name">Asistente JenApp</div>
            <div className="chat-jenapp-status">
              <span className="chat-jenapp-status-dot" />
              En línea
            </div>
          </div>
        </div>
        <button onClick={() => setVisible(false)}>✕</button>
      </div>

      <div className="chat-jenapp-messages">
        {mensajes.length === 0 && (
          <div className="chat-jenapp-empty">
            Pregúntame cosas como:
            <br />
            <i>“¿Que productos estan bajo de stock?”</i>
            <br />
            <i>“¿Cuanto se ha vendido hoy?”</i>
            <br />
            <i>“¿cuales productos se han vendido hoy?”</i>
            <br />
            <i>“¿Como ha sido la venta los ultimos dias?”</i>
            <br />
            <i>“¿Dónde veo el reporte de ventas?”</i>
          </div>
        )}

        {mensajes.map((m, i) => (
          <div
            key={i}
            className={
              "chat-msg " +
              (m.de === "usuario" ? "chat-msg-user" : "chat-msg-bot")
            }
          >
            {m.texto}
          </div>
        ))}

        {cargando && (
          <div className="chat-msg-typing">Escribiendo...</div>
        )}
      </div>

      <div className="chat-jenapp-input">
        <textarea
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          rows={2}
          placeholder="Escribe tu duda sobre JenApp..."
        />
        <button
          onClick={enviar}
          disabled={cargando || !input.trim()}
          className={
            "chat-send-btn " +
            (cargando || !input.trim() ? "disabled" : "enabled")
          }
        >
          ▶
        </button>
      </div>
    </div>
  );
}
