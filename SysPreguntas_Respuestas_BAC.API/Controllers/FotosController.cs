using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SysPreguntas_Respuestas_BAC.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SysPreguntas_Respuestas_BAC.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FotosController : ControllerBase
    {
        private readonly BDContext _context;

        public FotosController(BDContext context)
        {
            _context = context;
        }
        #region CRUD 
        #region CREAR

        [HttpPost("crear")]
        public async Task<IActionResult> CrearFotoPregunta([FromBody] CrearFotoPreguntaDto fotoPreguntaDto)
        {
            try
            {
                if (fotoPreguntaDto == null)
                {
                    return BadRequest("Datos inválidos.");
                }

                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("CrearFotoPregunta", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@IdPregunta", fotoPreguntaDto.IdPregunta);
                        command.Parameters.AddWithValue("@UrlFoto", fotoPreguntaDto.Url);

                        var idFotoPregunta = await command.ExecuteScalarAsync();

                        var fotoPregunta = new FotosPreguntas
                        {
                            IdFotoPregunta = Convert.ToInt32(idFotoPregunta),
                            IdPregunta = fotoPreguntaDto.IdPregunta,
                            UrlFoto = fotoPreguntaDto.Url
                        };

                        return Ok(fotoPregunta);
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion

        #region ObtenerFotosPorIdPregunta
        [HttpGet("obtener-por-pregunta/{idPregunta}")]
        public async Task<IActionResult> ObtenerFotosPorIdPregunta(int idPregunta)
        {
            try
            {
                var fotos = new List<FotosPreguntas>();
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("ObtenerFotosPorIdPregunta", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@IdPregunta", idPregunta);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                fotos.Add(new FotosPreguntas
                                {
                                    IdFotoPregunta = reader.GetInt32(0),
                                    IdPregunta = reader.GetInt32(1),
                                    UrlFoto = reader.GetString(2)
                                });
                            }
                        }
                    }
                }

                if (fotos == null || fotos.Count == 0)
                    return NotFound("No se encontraron fotos para la pregunta especificada.");

                var opcionesJson = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    PropertyNameCaseInsensitive = true
                };

                return Ok(JsonSerializer.Serialize(fotos, opcionesJson));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion

        #region OBTENER TODOS

        [HttpGet("obtener")]
        public async Task<IActionResult> ObtenerFotosPreguntas()
        {
            try
            {
                var fotos = new List<FotosPreguntasDto>();

                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("ObtenerFotosPreguntas", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var fotoPregunta = new FotosPreguntasDto
                                {
                                    IdFotoPregunta = reader.GetInt32(reader.GetOrdinal("IdFotoPregunta")),
                                    IdPregunta = reader.GetInt32(reader.GetOrdinal("IdPregunta")),
                                    UrlFoto = reader.GetString(reader.GetOrdinal("UrlFoto")),
                                    TituloPregunta = reader.GetString(reader.GetOrdinal("TituloPregunta"))
                                };

                                fotos.Add(fotoPregunta);
                            }
                        }
                    }
                }

                return Ok(fotos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion
        #endregion
    }
    public class CrearFotoPreguntaDto
    {
        public int IdPregunta { get; set; }
        public string Url { get; set; }
    }

}
