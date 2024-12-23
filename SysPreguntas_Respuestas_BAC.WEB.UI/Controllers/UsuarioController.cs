using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SysPreguntas_Respuestas_BAC.API.Models;
using SysPreguntas_Respuestas_BAC.API.Auth;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Azure.Core;

namespace SysPreguntas_Respuestas_BAC.WEB.UI.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly HttpClient _httpClient;

        public UsuarioController(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("API");
        }

        [AllowAnonymous]
        public IActionResult Login(string ReturnUrl = null)
        {
            ViewBag.Url = ReturnUrl;
            ViewBag.Error = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(Usuarios pUsuario, string pReturnUrl = null)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(pUsuario);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Usuarios/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, responseData["Login"].ToString()),
                new Claim("Token", responseData["Token"].ToString()),
                new Claim("NombreUsuario", $"{responseData["Nombre"]} {responseData["Apellido"]}"),
                new Claim("CorreoUsuario", responseData["Login"].ToString()),
                new Claim("urlUsuario", responseData["urlfoto"].ToString()),
                new Claim("UserId", responseData["IdUsuario"].ToString())
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    if (!string.IsNullOrWhiteSpace(pReturnUrl))
                        return Redirect(pReturnUrl);
                    else
                        return RedirectToAction("Index", "Home");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Credenciales incorrectas");
                }
                else
                {
                    throw new Exception("Error al comunicarse con el servidor de autenticación.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Url = pReturnUrl;
                ViewBag.Error = ex.Message;
                return View(pUsuario);
            }
        }



        [AllowAnonymous]
        public async Task<IActionResult> CerrarSesion(string ReturnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Usuario");
        }

        [AllowAnonymous]
        public IActionResult Register(string ReturnUrl = null)
        {
            ViewBag.Url = ReturnUrl;
            ViewBag.Error = "";
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register( Usuarios pUsuario, string pReturnUrl = null, IFormFile imagenFile = null)
        {
            try
            {
                // Validar si los datos son correctos
                if (!ModelState.IsValid)
                {
                    throw new Exception("Por favor, completa todos los campos obligatorios correctamente.");
                }
                if (imagenFile != null && imagenFile.Length > 0)
                {
                    // Obtener la ruta donde se guardará la imagen (en wwwroot/images)
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", imagenFile.FileName);

                    // Guardar la imagen en el servidor
                    using (var fileStream = new FileStream(imagePath, FileMode.Create))
                    {
                        await imagenFile.CopyToAsync(fileStream);
                    }

                    // Asignar la URL relativa al objeto usuario
                    pUsuario.UrlFoto = $"/images/{imagenFile.FileName}";
                }
                else
                {
                    // Si no se sube una imagen, mantener la propiedad UrlFoto como null
                    pUsuario.UrlFoto = null;
                }
                // Serializar el objeto usuario a JSON
                var jsonContent = JsonSerializer.Serialize(pUsuario);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Llamar a la API
                var response = await _httpClient.PostAsync("/api/Usuarios/crear", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Registro completado con éxito.";
                    return RedirectToAction("Login", "Usuario");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error en el registro: {errorResponse}");
                }
                else
                {
                    throw new Exception("Error al comunicarse con el servidor.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Url = pReturnUrl;
                ViewBag.Error = ex.Message;
                return View(pUsuario);
            }
        }



    }
 
}
