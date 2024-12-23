using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SysPreguntas_Respuestas_BAC.API.Modelss;

[Index("Login", Name = "UQ_Usuarios_Login", IsUnique = true)]
[Index("Usuario", Name = "UQ__Usuarios__E3237CF7F221F62B", IsUnique = true)]
public partial class Usuarios
{
    [Key]
    public int IdUsuario { get; set; }

    [StringLength(50)]
    public string Usuario { get; set; } = null!;

    [StringLength(100)]
    public string Contrasena { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    [StringLength(50)]
    public string Apellido { get; set; } = null!;

    [StringLength(255)]
    public string? UrlFoto { get; set; }

    public byte? Estado { get; set; }

    [StringLength(100)]
    public string Login { get; set; } = null!;

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Preguntas> Preguntas { get; set; } = new List<Preguntas>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Respuestas> Respuestas { get; set; } = new List<Respuestas>();
}
