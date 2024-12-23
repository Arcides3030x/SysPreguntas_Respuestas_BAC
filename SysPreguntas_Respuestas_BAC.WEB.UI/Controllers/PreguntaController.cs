using Microsoft.AspNetCore.Mvc;
using SysPreguntas_Respuestas_BAC.API.Controllers;
using SysPreguntas_Respuestas_BAC.API.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SysPreguntas_Respuestas_BAC.WEB.UI.Controllers
{
    public class PreguntaController : Controller
    {
        private readonly ILogger<PreguntaController> _logger;
        private readonly HttpClient _httpClient;

        public PreguntaController(IHttpClientFactory clientFactory, ILogger<PreguntaController> logger)
        {
            _httpClient = clientFactory.CreateClient("API");
            _logger = logger;
        }
        public async Task<IActionResult> Index(int id)
        {
            try
            {
                var token = User.FindFirst("Token")?.Value;

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("El token no está presente o es inválido.");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var responseRespuestas = await _httpClient.GetAsync($"/api/Respuestas/obtener-por-pregunta/{id}");

                List<Respuestas>? respuestas = null;
                if (responseRespuestas.IsSuccessStatusCode)
                {
                    var jsonResponseRespues = await responseRespuestas.Content.ReadAsStringAsync();
                    respuestas = JsonSerializer.Deserialize<List<Respuestas>>(jsonResponseRespues, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                var responsePregunta = await _httpClient.GetAsync($"/api/Preguntas/obtener/{id}");
                if (!responsePregunta.IsSuccessStatusCode)
                {
                    return StatusCode((int)responsePregunta.StatusCode, "Error al obtener la pregunta desde la API.");
                }

                var jsonResponsePregunta = await responsePregunta.Content.ReadAsStringAsync();
                var pregunta = JsonSerializer.Deserialize<Preguntas>(jsonResponsePregunta, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var responseFotos = await _httpClient.GetAsync($"/api/Fotos/obtener-por-pregunta/{id}");
                if (responseFotos.IsSuccessStatusCode)
                {
                    var jsonResponseFotos = await responseFotos.Content.ReadAsStringAsync();
                    var fotos = JsonSerializer.Deserialize<List<FotosPreguntas>>(jsonResponseFotos, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    pregunta.FotosPreguntas = fotos;
                }

                ViewBag.Pregunta = pregunta;
                ViewBag.IdPregunta = id;

                return View(respuestas);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Responder(int id, string contenido, int? idRespuestaPadre = null)
        {
            try
            {
                var token = User.FindFirst("Token")?.Value;

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("El token no está presente o es inválido.");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var nuevaRespuesta = new
                {
                    Contenido = contenido,
                    IdPregunta = id,
                    IdUsuario = int.Parse(User.FindFirst("UserId")?.Value ?? "0"),
                    IdRespuestaPadre = idRespuestaPadre
                };

                var jsonContent = JsonSerializer.Serialize(nuevaRespuesta);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Respuestas/crear", content);


                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", new { id });
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Error al enviar la respuesta: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }
        public async Task<IActionResult> MisPreguntas(string order = "desc")
        {
            try
            {
                var token = User.FindFirst("Token")?.Value;
                int IdUsuario = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                if (IdUsuario == 0)
                {
                    return Unauthorized("El ID de usuario no es válido.");
                }


                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync($"/api/Preguntas/obtener-por-usuario/{IdUsuario}");

                List<Preguntas>? preguntas = null;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    preguntas = JsonSerializer.Deserialize<List<Preguntas>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    return View();
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

        [HttpPost]
        public async Task<IActionResult> Cerrar(int idPregunta)
        {
            try
            {
                var token = User.FindFirst("Token")?.Value;

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("El token no está presente o es inválido.");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(JsonSerializer.Serialize(new { IdPregunta = idPregunta }), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Preguntas/cerrarPregunta", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Error al cerrar la pregunta desde la API: {error}");
                }

                return RedirectToAction("MisPreguntas", new { order = "desc" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }



        public async Task<IActionResult> Crear()
        {
          
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Preguntas pregunta, List<IFormFile> imagenFile)
        {
            try
            {
                foreach (var file in imagenFile)
                {
                    if (!file.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("imagenFile", "Todos los archivos deben ser imágenes.");
                        return RedirectToAction("Index", "Home");
                    }
                }

                var token = User.FindFirst("Token")?.Value;
                var idUsuario = User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(idUsuario))
                {
                    return Unauthorized("El token o el ID de usuario no están presentes o son inválidos.");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                pregunta.IdUsuario = int.Parse(idUsuario);

                var preguntaPayload = JsonSerializer.Serialize(new
                {
                    IdUsuario = pregunta.IdUsuario,
                    Contenido = pregunta.Contenido,
                    Titulo = pregunta.Titulo
                });

                var preguntaContent = new StringContent(preguntaPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Preguntas/crear", preguntaContent);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Error al crear la pregunta: {errorResponse}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var nuevaPregunta = JsonSerializer.Deserialize<Preguntas>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (nuevaPregunta == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la respuesta del servidor.");
                }

                if (imagenFile != null && imagenFile.Any())
                {
                    var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                    if (!Directory.Exists(uploadDirectory))
                    {
                        Directory.CreateDirectory(uploadDirectory);
                    }

                    foreach (var file in imagenFile)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadDirectory, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var imageUrl = $"/images/{fileName}";

                        var fotoPayload = JsonSerializer.Serialize(new
                        {
                            IdPregunta = nuevaPregunta.IdPregunta,
                            Url = imageUrl
                        });

                        var fotoContent = new StringContent(fotoPayload, Encoding.UTF8, "application/json");
                        var fotoResponse = await _httpClient.PostAsync("/api/Fotos/crear", fotoContent);

                        if (!fotoResponse.IsSuccessStatusCode)
                        {
                            var errorFoto = await fotoResponse.Content.ReadAsStringAsync();
                            return StatusCode((int)fotoResponse.StatusCode, $"Error al subir imagen: {errorFoto}");
                        }
                    }
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }
    }
}
