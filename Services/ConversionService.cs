using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonedaApi.Services
{
    public class ConversionService
    {
        private readonly HttpClient _httpClient;

        public ConversionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<double> ObtenerCotizacionDolarBlue()
        {
            string endpoint = "https://dolarapi.com/v1/dolares/blue";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(endpoint);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var cotizacion = JsonSerializer.Deserialize<CotizacionResponse>(responseBody);

                if (cotizacion == null || cotizacion.venta <= 0)
                {
                    throw new HttpRequestException("Error al obtener la cotización.");
                }

                return cotizacion.venta;
            }
        }

        public async Task<double> ConvertirADolares(double dolares)
        {
            double valorDolar = await ObtenerCotizacionDolarBlue();
            return dolares * valorDolar;
        }
    }

    public class CotizacionResponse
    {
        public string moneda { get; set; }
        public string casa { get; set; }
        public string nombre { get; set; }
        public double compra { get; set; }
        public double venta { get; set; }
        public string fechaActualizacion { get; set; }
    }
}
