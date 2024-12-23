using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SysPreguntas_Respuestas_BAC.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using System.Data;


namespace SysPreguntas_Respuestas_BAC.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PreguntasController : ControllerBase
    {
        private readonly BDContext _context;

        public PreguntasController(BDContext context)
        {
            _context = context;
        }
        #region CRUD 
        #region CREAR

        [HttpPost("crear")]
        public async Task<IActionResult> CrearPregunta([FromBody] CrearPreguntaRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("La solicitud no contiene los datos requeridos.");
                }

                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("CrearPregunta", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                        command.Parameters.AddWithValue("@Titulo", request.Titulo);
                        command.Parameters.AddWithValue("@Contenido", request.Contenido);

                        var idPregunta = await command.ExecuteScalarAsync();

                        if (idPregunta != null)
                        {
                            return Ok(new { IdPregunta = idPregunta });
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear la pregunta.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #region Cerrar Pregunta
        [HttpPost("cerrarPregunta")]
        public async Task<IActionResult> CerrarPregunta([FromBody] CerrarPreguntaRequest request)
        {
            try
            {
                if (request == null || request.IdPregunta == 0)
                {
                    return BadRequest("El ID de la pregunta es requerido.");
                }

                var idPreguntaParam = new SqlParameter("@IdPregunta", request.IdPregunta);

                var result = await _context.Database.ExecuteSqlRawAsync("EXEC EditarEstadoPregunta @IdPregunta", idPreguntaParam);

                if (result == 0)
                {
                    return NotFound("Pregunta no encontrada o ya está cerrada.");
                }

                return Ok(new { mensaje = "Pregunta cerrada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        #endregion
        #endregion
        #region OBTENER TODOS
        [HttpGet("obtener")]
        public async Task<IActionResult> ObtenerPreguntas()
        {
            try
            {
                var preguntas = await _context.Preguntas.FromSqlRaw("EXEC ObtenerPreguntasActivas").ToListAsync();

                foreach (var pregunta in preguntas)
                {
                    pregunta.IdUsuarioNavigation = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.IdUsuario == pregunta.IdUsuario);

                }
                var opcionesJson = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    PropertyNameCaseInsensitive = true
                };

                return Ok(JsonSerializer.Serialize(preguntas, opcionesJson));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion

        #region OBTENER POR ID
        [HttpGet("obtener/{id}")]
        public async Task<IActionResult> ObtenerPreguntaPorId(int id)
        {
            try
            {
                var pregunta = await _context.Preguntas
                    .Include(p => p.FotosPreguntas)
                    .Include(p => p.Respuestas)
                    .Include(p => p.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(p => p.IdPregunta == id && p.Estado == 1);

                if (pregunta == null)
                {
                    return NotFound("Pregunta no encontrada o inactiva.");
                }


                var opcionesJson = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                return Ok(JsonSerializer.Serialize(pregunta, opcionesJson));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion

        #region OBTENER POR ID DE USUARIO


        [HttpGet("obtener-por-usuario/{idUsuario}")]
        public async Task<IActionResult> ObtenerPreguntasPorUsuario(int idUsuario)
        {
            try
            {
                var preguntas = new List<Preguntas>();

                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("ObtenerPreguntasPorUsuario", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@IdUsuario", idUsuario);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var pregunta = new Preguntas
                                {
                                    IdPregunta = reader.GetInt32(reader.GetOrdinal("IdPregunta")),
                                    Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
                                    Contenido = reader.GetString(reader.GetOrdinal("Contenido")),
                                    IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),
                                    EstaCerrada = reader.IsDBNull(reader.GetOrdinal("EstaCerrada")) ? null : reader.GetBoolean(reader.GetOrdinal("EstaCerrada")),
                                    FechaCreacion = reader.IsDBNull(reader.GetOrdinal("FechaCreacion")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                                    Estado = reader.GetByte(reader.GetOrdinal("Estado")),
                                    FotosPreguntas = new List<FotosPreguntas>(),
                                    Respuestas = new List<Respuestas>()
                                };

                                if (!reader.IsDBNull(reader.GetOrdinal("IdFotoPregunta")))
                                {
                                    var foto = new FotosPreguntas
                                    {
                                        IdFotoPregunta = reader.GetInt32(reader.GetOrdinal("IdFotoPregunta")),
                                        UrlFoto = reader.GetString(reader.GetOrdinal("UrlFoto"))
                                    };
                                    pregunta.FotosPreguntas.Add(foto);
                                }

                                if (!reader.IsDBNull(reader.GetOrdinal("IdRespuesta")))
                                {
                                    var respuesta = new Respuestas
                                    {
                                        IdRespuesta = reader.GetInt32(reader.GetOrdinal("IdRespuesta")),
                                        Contenido = reader.GetString(reader.GetOrdinal("RespuestaContenido")),
                                        FechaCreacion = reader.GetDateTime(reader.GetOrdinal("RespuestaFechaCreacion"))
                                    };
                                    pregunta.Respuestas.Add(respuesta);
                                }

                                preguntas.Add(pregunta);
                            }
                        }
                    }
                }

                if (preguntas == null || !preguntas.Any())
                {
                    return NotFound("No se encontraron preguntas para el usuario especificado o están inactivas.");
                }

                var opcionesJson = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                return Ok(JsonSerializer.Serialize(preguntas, opcionesJson));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion
        #endregion
    }
    
}
