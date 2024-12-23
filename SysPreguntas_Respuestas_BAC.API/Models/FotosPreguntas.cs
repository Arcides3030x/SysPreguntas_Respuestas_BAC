using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SysPreguntas_Respuestas_BAC.API.Models;

public partial class FotosPreguntas
{
    [Key]
    public int IdFotoPregunta { get; set; }

    public int IdPregunta { get; set; }

    [StringLength(255)]
    public string UrlFoto { get; set; } = null!;

    [ForeignKey("IdPregunta")]
    [InverseProperty("FotosPreguntas")]
    public virtual Preguntas IdPreguntaNavigation { get; set; } = null!;
}
public class FotosPreguntasDto
{
    public int IdFotoPregunta { get; set; }
    public int IdPregunta { get; set; }
    public string UrlFoto { get; set; }
    public string TituloPregunta { get; set; }
}
