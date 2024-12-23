using Microsoft.IdentityModel.Tokens;
using SysPreguntas_Respuestas_BAC.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace SysPreguntas_Respuestas_BAC.API.Auth
{
    public class JwtAuthentication
    {
            private readonly string _key;

            public JwtAuthentication(string key)
            {
                _key = key;
            }
  

            public string EncryptWithMD5(string password)
            {
                using (var md5 = MD5.Create())
                {
                    var result = md5.ComputeHash(Encoding.ASCII.GetBytes(password));
                    var strResult = "";
                    for (int i = 0; i < result.Length; i++)
                    {
                        strResult += result[i].ToString("x2").ToLower();
                        password = strResult;

                    }
                    return password;
                }
            }


            public string Authenticate(Usuarios pUsuario)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.ASCII.GetBytes(_key);

                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, pUsuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, pUsuario.Nombre),
                new Claim(ClaimTypes.Surname, pUsuario.Apellido),
            };

          
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),

                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
    }
}
