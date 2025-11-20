using System.Net.Http.Json;
using System.Text.Json;

namespace Webapi.Services
{
    public class ChatbotService
    {
        private readonly HttpClient _http;

        public ChatbotService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:11434")
            };
        }

        public async Task<string> ResponderAsync(string mensajeUsuario)
        {
            try
            {
                var request = new
                {
                    model = "llama3.1",  
                    messages = new[]
                    {
                        new { role = "system", content = "Eres el asistente oficial de JenApp, un sistema de ventas e inventario para pequeñas tiendas.\r\nResponde SIEMPRE en español, de forma breve, clara y paso a paso.\r\n\r\nFuncionalidades principales de JenApp:\r\n- Módulo de productos: crear, editar, eliminar y listar productos (nombre, precio, stock).\r\n- Módulo de ventas: registrar ventas, elegir productos, cantidades y total.\r\n- Módulo de reportes: ver ventas por día, rango de fechas y total vendido.\r\n- Módulo de inventario: ver stock actual, productos agotados o por agotarse.\r\n- Módulo de usuarios: login, registro y roles básicos.\r\n\r\nReglas:\r\n- Si te preguntan “cómo hacer algo”, responde con pasos numerados dentro de JenApp.\r\n- Si te preguntan algo que JenApp NO hace, respóndelo, pero aclara: \r\n  'Esto no está aún dentro de JenApp, pero en general podrías...'\r\n- Si no entiendes la pregunta, pide que la reformulen." },
                        new { role = "user",   content = mensajeUsuario }
                    },
                    stream = false
                };

                var response = await _http.PostAsJsonAsync("/api/chat", request);

                if (!response.IsSuccessStatusCode)
                    return $"Error llamando a Ollama: {response.StatusCode}";

                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var content = doc.RootElement
                                 .GetProperty("message")
                                 .GetProperty("content")
                                 .GetString();

                return content ?? "No pude generar respuesta.";
            }
            catch (Exception ex)
            {
                return $"Error al conectar con Ollama: {ex.Message}";
            }
        }
    }
}

