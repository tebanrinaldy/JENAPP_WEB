using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Webapi.Data;
using Webapi.Models; 

namespace Webapi.Services
{
    public class ChatbotService
    {
        private readonly Connectioncontextdb _context;
        private readonly HttpClient _ollama;

        public ChatbotService(Connectioncontextdb context, IHttpClientFactory httpFactory)
        {
            _context = context;
            _ollama = httpFactory.CreateClient("ollama");
        }

    
        public async Task<string> ResponderAsync(string mensajeUsuario)
        {
            var intent = await ClasificarIntentAsync(mensajeUsuario);

            switch (intent)
            {
                case "VENTAS_HOY":
                    return await ResponderVentasHoyAsync();

                case "TOP_PRODUCTOS_SEMANA":
                    return await ResponderTopProductosSemanaAsync();

                case "STOCK_BAJO":
                    return await ResponderStockBajoAsync();

                

                default:
                    return await ResponderChatGeneralAsync(mensajeUsuario);
            }
        }

      
        private async Task<string> ClasificarIntentAsync(string pregunta)
        {
            var prompt = $@"
Eres un clasificador de intenciones para un sistema POS llamado JenApp.
SOLO debes responder uno de estos códigos EXACTOS:

VENTAS_HOY
VENTAS_RANGO
TOP_PRODUCTOS_SEMANA
STOCK_BAJO

GENERAL

Reglas:
- No expliques nada.
- No añadas texto extra.
- No inventes datos.
- No digas ni inventes nada que no este en el programa
- Si la pregunta menciona dos fechas: VENTAS_RANGO.
- Si pregunta por ventas del día o de hoy: VENTAS_HOY.
- Si pregunta por semana o últimos 7 días: TOP_PRODUCTOS_SEMANA.
- Si pregunta por stock, inventario bajo o productos que se acaban: STOCK_BAJO.
- Si ninguna aplica: GENERAL.

Pregunta del usuario: ""{pregunta}""
Respuesta SOLO con el código:
";

            var texto = await LlamarOllamaSimpleAsync(prompt);
            return texto.Trim().ToUpper();
        }

        private async Task<string> ResponderVentasHoyAsync()
        {
            var hoy = DateTime.Today;

            var ventasHoy = await _context.Sales
                .Where(s => s.Date.Date == hoy)
                .ToListAsync();

            if (!ventasHoy.Any())
                return "Hoy aún no se han registrado ventas.";

            var total = ventasHoy.Sum(v => v.Total);
            var cantidad = ventasHoy.Count;

            return $"Hoy has registrado {cantidad} ventas por un total de {total:C}.";
        }

        private async Task<string> ResponderTopProductosSemanaAsync()
        {
            var hoy = DateTime.Today;
            var inicioSemana = hoy.AddDays(-7);

            var query = await _context.SaleDetails
                .Where(d => d.Sale.Date >= inicioSemana && d.Sale.Date <= hoy)
                .GroupBy(d => d.Product)
                .Select(g => new
                {
                    Producto = g.Key.Name,
                    Cantidad = g.Sum(x => x.Quantity),
                    Total = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToListAsync();

            if (!query.Any())
                return "En los últimos 7 días no se han registrado ventas.";

            var lineas = query.Select(x =>
                $"- {x.Producto}: {x.Cantidad} unidades ({x.Total:C})");

            return "Top 5 productos más vendidos en los últimos 7 días:\n" +
                   string.Join("\n", lineas);
        }

        private async Task<string> ResponderStockBajoAsync()
        {
            const int limite = 5;

            var productos = await _context.Products
                .Where(p => p.Stock <= limite)
                .OrderBy(p => p.Stock)
                .ToListAsync();

            if (!productos.Any())
                return "No tienes productos con stock bajo en este momento.";

            var lineas = productos.Select(p =>
                $"- {p.Name}: {p.Stock} unidades restantes");

            return "Productos con stock bajo:\n" + string.Join("\n", lineas);
        }

       
        private async Task<string> ResponderChatGeneralAsync(string mensaje)
        {
            var prompt = $@"
Eres el asistente de JenApp, un sistema de ventas e inventario.
Responde SIEMPRE en español, de forma corta y clara.
Si no tienes datos en tiempo real, responde de manera general.

Pregunta del usuario:
{mensaje}
";

            return await LlamarOllamaSimpleAsync(prompt);
        }

      
        private async Task<string> LlamarOllamaSimpleAsync(string prompt)
        {
            var body = new
            {
                model = "llama3.1",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                stream = false
            };

            var response = await _ollama.PostAsJsonAsync("/api/chat", body);

            if (!response.IsSuccessStatusCode)
                return $"Error al conectar con el modelo ({response.StatusCode}).";

            var data = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

            return data?.message?.content ?? "No pude generar una respuesta.";
        }

       
        public class OllamaChatResponse
        {
            public OllamaMessage? message { get; set; }
        }

        public class OllamaMessage
        {
            public string? role { get; set; }
            public string? content { get; set; }
        }
    }
}
