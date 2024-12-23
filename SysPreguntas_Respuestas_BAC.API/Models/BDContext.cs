using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SysPreguntas_Respuestas_BAC.API.Models;

public partial class BDContext : DbContext
{
    public BDContext()
    {
    }

    public BDContext(DbContextOptions<BDContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FotosPreguntas> FotosPreguntas { get; set; }

    public virtual DbSet<Preguntas> Preguntas { get; set; }

    public virtual DbSet<Respuestas> Respuestas { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Data Source=HP\\ARCIDES;Initial Catalog=PreguntasRespuestasDB;Integrated Security=True;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FotosPreguntas>(entity =>
        {
            entity.HasKey(e => e.IdFotoPregunta).HasName("PK__FotosPre__36663D6009C63B8A");

            entity.HasOne(d => d.IdPreguntaNavigation).WithMany(p => p.FotosPreguntas).HasConstraintName("FK__FotosPreg__IdPre__4222D4EF");
        });

        modelBuilder.Entity<Preguntas>(entity =>
        {
            entity.HasKey(e => e.IdPregunta).HasName("PK__Pregunta__754EC09E2FA06F35");

            entity.Property(e => e.EstaCerrada).HasDefaultValue(false);
            entity.Property(e => e.Estado).HasDefaultValue((byte)1);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Preguntas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Preguntas__IdUsu__3C69FB99");
        });

        modelBuilder.Entity<Respuestas>(entity =>
        {
            entity.HasKey(e => e.IdRespuesta).HasName("PK__Respuest__D34801985DD08A09");

            entity.Property(e => e.Estado).HasDefaultValue((byte)1);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdPreguntaNavigation).WithMany(p => p.Respuestas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Respuesta__IdPre__4CA06362");

            entity.HasOne(d => d.IdRespuestaPadreNavigation).WithMany(p => p.InverseIdRespuestaPadreNavigation).HasConstraintName("FK__Respuesta__IdRes__4E88ABD4");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Respuestas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Respuesta__IdUsu__4D94879B");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__5B65BF971CCF4A5A");

            entity.Property(e => e.Estado).HasDefaultValue((byte)1);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Login).HasDefaultValue("");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
