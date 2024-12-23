using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SysPreguntas_Respuestas_BAC.API.Models;

public partial class Preguntas
{
    [Key]
    public int IdPregunta { get; set; }

    [StringLength(255)]
    public string Titulo { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public int IdUsuario { get; set; }

    public bool? EstaCerrada { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    public byte? Estado { get; set; }

    [InverseProperty("IdPreguntaNavigation")]
    public virtual ICollection<FotosPreguntas> FotosPreguntas { get; set; } = new List<FotosPreguntas>();


    [ForeignKey("IdUsuario")]
    [InverseProperty("Preguntas")]
    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;

    [InverseProperty("IdPreguntaNavigation")]
    public virtual ICollection<Respuestas> Respuestas { get; set; } = new List<Respuestas>();
}
public class CrearPreguntaRequest
{
    public int IdUsuario { get; set; }
    public string Contenido { get; set; }
    public string Titulo { get; set; }
}
public class CerrarPreguntaRequest
{
    public int IdPregunta { get; set; }
}
