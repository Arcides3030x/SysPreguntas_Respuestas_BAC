using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SysPreguntas_Respuestas_BAC.API.Auth;
using SysPreguntas_Respuestas_BAC.API.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SysPreguntas_Respuestas_BAC.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private static readonly string _key = "ARCI2024SecretKeyForTokenAuthentication";
        private readonly JwtAuthentication jwtAuthentication = new JwtAuthentication(_key);
        private readonly BDContext _context;

        public UsuariosController(BDContext context)
        {
            _context = context;
        }
        #region CRUD 
        #region BUSCAR SI CORREO EXISTE
        private async Task<bool> ExisteLogin(Usuarios usuario)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Login == usuario.Login && u.IdUsuario != usuario.IdUsuario);
        }
        #endregion

        #region CREAR
        [HttpPost("crear")]
        [AllowAnonymous]
        public async Task<IActionResult> CrearUsuario([FromBody] Usuarios usuario)
        {
            try
            {
                var existeLogin = await _context.Usuarios
                    .AnyAsync(u => u.Login == usuario.Login);

                if (existeLogin)
                    return BadRequest("El usuario ya existe.");

                usuario.Contrasena = jwtAuthentication.EncryptWithMD5(usuario.Contrasena);

                var parameters = new[]
                {
            new SqlParameter("@Usuario", usuario.Usuario),
            new SqlParameter("@Contrasena", usuario.Contrasena),
            new SqlParameter("@Nombre", usuario.Nombre),
            new SqlParameter("@Apellido", usuario.Apellido),
            new SqlParameter("@UrlFoto", (object)usuario.UrlFoto ?? DBNull.Value),
            new SqlParameter("@Login", usuario.Login),
            new SqlParameter("@Estado", usuario.Estado ?? (byte)1), 
            new SqlParameter("@FechaRegistro", (object)usuario.FechaRegistro ?? DBNull.Value)
        };

                var idUsuario = await _context.Database.ExecuteSqlRawAsync("EXEC CrearUsuario @Usuario, @Contrasena, @Nombre, @Apellido, @UrlFoto, @Login, @Estado, @FechaRegistro", parameters);

                usuario.IdUsuario = idUsuario;
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

        #region LOGIN
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] object pUsuario)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            string strUsuario = JsonSerializer.Serialize(pUsuario);
            Usuarios usuario = JsonSerializer.Deserialize<Usuarios>(strUsuario, options);

            Usuarios usuario_auth = await _context.Usuarios
                .Where(x => x.Login == usuario.Login && x.Contrasena == jwtAuthentication.EncryptWithMD5(usuario.Contrasena))
                .FirstOrDefaultAsync();

            if (usuario_auth != null && usuario_auth.IdUsuario > 0 && usuario.Login == usuario_auth.Login)
            {
                var token = jwtAuthentication.Authenticate(usuario_auth);

                // Devolver un objeto anónimo
                return Ok(new Dictionary<string, object>
        {
            { "Token", token },
            { "IdUsuario", usuario_auth.IdUsuario },
            { "Login", usuario_auth.Login },
            { "Nombre", usuario_auth.Nombre },
            { "urlfoto", usuario_auth.UrlFoto ?? "https://flowbite.com/docs/images/people/profile-picture-5.jpg" },
            { "Apellido", usuario_auth.Apellido },
        });
            }
            else
            {
                return Unauthorized();
            }
        }

        #endregion

     
        #endregion
    }

    public class CambiarPasswordModel
    {
        public int IdUsuario { get; set; }
        public string PasswordAnterior { get; set; }
        public string PasswordNueva { get; set; }
    }
}

