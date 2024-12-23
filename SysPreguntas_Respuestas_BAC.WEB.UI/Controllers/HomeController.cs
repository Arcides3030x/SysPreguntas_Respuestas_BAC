using Microsoft.AspNetCore.Mvc;
using SysPreguntas_Respuestas_BAC.API.Models;
using SysPreguntas_Respuestas_BAC.WEB.UI.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace SysPreguntas_Respuestas_BAC.WEB.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(IHttpClientFactory clientFactory, ILogger<HomeController> logger)
        {
            _httpClient = clientFactory.CreateClient("API");
            _logger = logger;
        }


        public async Task<IActionResult> Index(string order = "desc")
        {
            try
            {
                var token = User.FindFirst("Token")?.Value;

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("El token no está presente o es inválido.");
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var responsePreguntas = await _httpClient.GetAsync("/api/Preguntas/obtener");

                if (!responsePreguntas.IsSuccessStatusCode)
                {
                    return StatusCode((int)responsePreguntas.StatusCode, "Error al obtener preguntas desde la API.");
                }

                var jsonResponsePreguntas = await responsePreguntas.Content.ReadAsStringAsync();
                var preguntas = JsonSerializer.Deserialize<List<Preguntas>>(jsonResponsePreguntas, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                foreach (var pregunta in preguntas)
                {
                    var responseFotos = await _httpClient.GetAsync($"/api/Fotos/obtener-por-pregunta/{pregunta.IdPregunta}");

                    if (responseFotos.IsSuccessStatusCode)
                    {
                        var jsonResponseFotos = await responseFotos.Content.ReadAsStringAsync();
                        var fotos = JsonSerializer.Deserialize<List<FotosPreguntas>>(jsonResponseFotos, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        pregunta.FotosPreguntas = fotos;
                    }
                }
                preguntas = order.ToLower() == "asc"
                   ? preguntas.OrderBy(p => p.FechaCreacion).ToList()
                   : preguntas.OrderByDescending(p => p.FechaCreacion).ToList();

                ViewBag.CurrentOrder = order;


                return View(preguntas);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
