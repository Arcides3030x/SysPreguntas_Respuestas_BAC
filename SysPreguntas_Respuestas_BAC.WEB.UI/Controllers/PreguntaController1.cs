using Microsoft.AspNetCore.Mvc;

namespace SysPreguntas_Respuestas_BAC.WEB.UI.Controllers
{
    public class PreguntaController1 : Controller
    {
        public IActionResult Index(int id)
        {
            // Puedes procesar el ID aquí (por ejemplo, obtener detalles de la pregunta)
            ViewBag.IdPregunta = id;
            return View();
        }
    }
}
