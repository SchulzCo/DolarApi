using Microsoft.AspNetCore.Mvc;
using MonedaApi.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonedaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonedaController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ConversionService _conversionService;

        public MonedaController(HttpClient httpClient, ConversionService conversionService)
        {
            _httpClient = httpClient;
            _conversionService = conversionService;
        }

        [HttpGet("cotizacion-blue")]
        public async Task<IActionResult> GetDolarBlue()
        {
            string responseBody = string.Empty;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://dolarapi.com/v1/dolares/blue");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();
            }

            return Ok(responseBody);
        }

        [HttpPost("cotizacion")]
        public async Task<IActionResult> GetCotizacion([FromBody] Currency currency)
        {
            string endpoint = currency.Code switch
            {
                "Bolsa" => "https://dolarapi.com/v1/dolares/bolsa",
                "Blue" => "https://dolarapi.com/v1/dolares/blue",
                "Cripto" => "https://dolarapi.com/v1/dolares/cripto",
                _ => null
            };

            if (endpoint == null)
                return BadRequest("Código de moneda no válido.");

            string responseBody = string.Empty;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(endpoint);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();
            }

            return Ok(responseBody);
        }

        [HttpGet("convertir-a-pesos")]
        public async Task<IActionResult> ConvertirAPesos([FromQuery] double dolares)
        {
            try
            {
                double pesos = await _conversionService.ConvertirADolares(dolares);
                return Ok(pesos);
            }
            catch (HttpRequestException ex)
            {
                return BadRequest($"Excepción de solicitud HTTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Excepción general: {ex.Message}");
            }
        }
    }

    public class Currency
    {
        public string Code { get; set; }
    }

    public class CotizacionResponse
    {
        public string valor { get; set; }
    }
}
