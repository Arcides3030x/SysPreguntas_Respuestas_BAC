using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SysPreguntas_Respuestas_BAC.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;

namespace SysPreguntas_Respuestas_BAC.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RespuestasController : ControllerBase
    {
        private readonly BDContext _context;

        public RespuestasController(BDContext context)
        {
            _context = context;
        }

        #region CRUD 
        #region CREAR
        [HttpPost("crear")]
        public async Task<IActionResult> CrearRespuesta([FromBody] RespuestaCrear respuestaDto)
        {
            try
            {
                if (respuestaDto == null || string.IsNullOrWhiteSpace(respuestaDto.Contenido))
                {
                    return BadRequest("El contenido de la respuesta no puede estar vacío.");
                }

                var idRespuestaParametro = new SqlParameter
                {
                    ParameterName = "@IdRespuesta",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                var parametros = new[]
                {
            new SqlParameter("@Contenido", respuestaDto.Contenido),
            new SqlParameter("@IdPregunta", respuestaDto.IdPregunta),
            new SqlParameter("@IdUsuario", respuestaDto.IdUsuario),
            new SqlParameter("@IdRespuestaPadre", respuestaDto.IdRespuestaPadre ?? (object)DBNull.Value),
            new SqlParameter("@Estado", 1),
            idRespuestaParametro
        };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC CrearRespuesta @Contenido, @IdPregunta, @IdUsuario, @IdRespuestaPadre, @Estado, @IdRespuesta OUTPUT",
                    parametros);

                var idRespuesta = (int)idRespuestaParametro.Value;

                return Ok(new
                {
                    Message = "Respuesta creada correctamente.",
                    RespuestaId = idRespuesta,
                    PreguntaId = respuestaDto.IdPregunta,
                    RespuestaPadreId = respuestaDto.IdRespuestaPadre
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }




        #endregion
        #region ObtenerRespuestasPorPregunta
        [HttpGet("obtener-por-pregunta/{idPregunta}")]
        public async Task<IActionResult> ObtenerRespuestasPorPregunta(int idPregunta)
        {
            try
            {
                var parametro = new SqlParameter("@IdPregunta", idPregunta);

                var respuestas = await _context.Respuestas
                    .FromSqlRaw("EXEC ObtenerRespuestasPorPregunta @IdPregunta", parametro)
                    .ToListAsync();

                // Reconstruir las relaciones manualmente
                foreach (var respuesta in respuestas)
                {
                    respuesta.IdUsuarioNavigation = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.IdUsuario == respuesta.IdUsuario);

                    if (respuesta.IdRespuestaPadre.HasValue)
                    {
                        respuesta.IdRespuestaPadreNavigation = await _context.Respuestas
                            .FirstOrDefaultAsync(r => r.IdRespuesta == respuesta.IdRespuestaPadre);
                    }
                }

                if (respuestas == null || !respuestas.Any())
                {
                    return NotFound($"No se encontraron respuestas para la pregunta con Id {idPregunta}.");
                }
                var opcionesJson = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                return Ok(JsonSerializer.Serialize(respuestas, opcionesJson));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        #endregion
        #region OBTENER TODOS
        [HttpGet("obtener")]
        public async Task<IActionResult> ObtenerRespuestas()
        {
            try
            {
                // Ejecutar el procedimiento almacenado
                var respuestas = await _context.Respuestas
                    .FromSqlRaw("EXEC ObtenerRespuestas")
                    .ToListAsync();

                return Ok(respuestas);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion
        #endregion
    }
    public class RespuestaCrear
    {
        [Required]
        public string Contenido { get; set; } = null!;

        [Required]
        public int IdPregunta { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        public int? IdRespuestaPadre { get; set; }
    }
}

