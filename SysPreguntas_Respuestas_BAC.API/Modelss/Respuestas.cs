using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SysPreguntas_Respuestas_BAC.API.Modelss;

public partial class Respuestas
{
    [Key]
    public int IdRespuesta { get; set; }

    public string Contenido { get; set; } = null!;

    public int IdPregunta { get; set; }

    public int IdUsuario { get; set; }

    public int? IdRespuestaPadre { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    public byte? Estado { get; set; }

    [ForeignKey("IdPregunta")]
    [InverseProperty("Respuestas")]
    public virtual Preguntas IdPreguntaNavigation { get; set; } = null!;

    [ForeignKey("IdRespuestaPadre")]
    [InverseProperty("InverseIdRespuestaPadreNavigation")]
    public virtual Respuestas? IdRespuestaPadreNavigation { get; set; }

    [ForeignKey("IdUsuario")]
    [InverseProperty("Respuestas")]
    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;

    [InverseProperty("IdRespuestaPadreNavigation")]
    public virtual ICollection<Respuestas> InverseIdRespuestaPadreNavigation { get; set; } = new List<Respuestas>();
}
