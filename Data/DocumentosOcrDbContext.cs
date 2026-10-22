using DocumentosOCRApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentosOCRApi.Data
{
    public class DocumentosOcrDbContext : DbContext
    {
        public DocumentosOcrDbContext(
            DbContextOptions<DocumentosOcrDbContext> options)
            : base(options)
        {
        }

        public DbSet<CatalogoTipoDocumento> TiposDocumento { get; set; }
        public DbSet<Proceso> Procesos { get; set; }
        public DbSet<DocumentoProceso> DocumentosProceso { get; set; }
        public DbSet<DatoExtraido> DatosExtraidos { get; set; }
        public DbSet<ValidacionDocumento> ValidacionesDocumento => Set<ValidacionDocumento>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CatalogoTipoDocumento>(entity =>
            {
                entity.ToTable("CAT_TIPO_DOCUMENTO");

                entity.HasKey(e => e.IdTipoDocumento);

                entity.Property(e => e.IdTipoDocumento)
                    .HasColumnName("ID_TIPO_DOCUMENTO");

                entity.Property(e => e.ClaveDocumento)
                    .HasColumnName("CLAVE_DOCUMENTO");

                entity.Property(e => e.NbDocumento)
                    .HasColumnName("NB_DOCUMENTO");

                entity.Property(e => e.RequiereOcr)
                    .HasColumnName("REQUIERE_OCR");

                entity.Property(e => e.StActivo)
                    .HasColumnName("ST_ACTIVO");

                entity.Property(e => e.FhRegistro)
                    .HasColumnName("FH_REGISTRO");
            });

            modelBuilder.Entity<Proceso>(entity =>
            {
                entity.ToTable("PROCESO");

                entity.HasKey(e => e.IdProceso);

                entity.Property(e => e.IdProceso)
                    .HasColumnName("ID_PROCESO");

                entity.Property(e => e.IdTipoPersona)
                    .HasColumnName("ID_TIPO_PERSONA");

                entity.Property(e => e.StProceso)
                    .HasColumnName("ST_PROCESO");

                entity.Property(e => e.FhInicio)
                    .HasColumnName("FH_INICIO");

                entity.Property(e => e.FhFin)
                    .HasColumnName("FH_FIN");
            });

            modelBuilder.Entity<DocumentoProceso>(entity =>
            {
                entity.ToTable("DOCUMENTO_PROCESO");

                entity.HasKey(e => e.IdDocumentoProceso);

                entity.Property(e => e.IdDocumentoProceso)
                    .HasColumnName("ID_DOCUMENTO_PROCESO");

                entity.Property(e => e.IdProceso)
                    .HasColumnName("ID_PROCESO");

                entity.Property(e => e.IdTipoDocumento)
                    .HasColumnName("ID_TIPO_DOCUMENTO");

                entity.Property(e => e.NbArchivo)
                    .HasColumnName("NB_ARCHIVO");

                entity.Property(e => e.TipoArchivo)
                    .HasColumnName("TIPO_ARCHIVO");

                entity.Property(e => e.NuTamanio)
                    .HasColumnName("NU_TAMANIO");

                entity.Property(e => e.TextoOcr)
                    .HasColumnName("TEXTO_OCR");

                entity.Property(e => e.StCargado)
                    .HasColumnName("ST_CARGADO");

                entity.Property(e => e.StOcrProcesado)
                    .HasColumnName("ST_OCR_PROCESADO");

                entity.Property(e => e.FhCarga)
                    .HasColumnName("FH_CARGA");
            });

            modelBuilder.Entity<DatoExtraido>(entity =>
            {
                entity.ToTable("DATOS_EXTRAIDOS_OCR");

                entity.HasKey(e => e.IdDatoExtraido);

                entity.Property(e => e.IdDatoExtraido)
                    .HasColumnName("ID_DATO_EXTRAIDO");

                entity.Property(e => e.IdDocumentoProceso)
                    .HasColumnName("ID_DOCUMENTO_PROCESO");

                entity.Property(e => e.NbCampo)
                    .HasColumnName("NB_CAMPO");

                entity.Property(e => e.NbValor)
                    .HasColumnName("NB_VALOR");

                entity.Property(e => e.FhRegistro)
                    .HasColumnName("FH_REGISTRO");
            });

            modelBuilder.Entity<ValidacionDocumento>(entity =>
            {
                entity.ToTable("VALIDACION_DOCUMENTO");

                entity.HasKey(e => e.IdValidacionDocumento);

                entity.Property(e => e.IdValidacionDocumento)
                    .HasColumnName("ID_VALIDACION_DOCUMENTO")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdDocumentoProceso)
                    .HasColumnName("ID_DOCUMENTO_PROCESO");

                entity.Property(e => e.CodigoRegla)
                    .HasColumnName("CODIGO_REGLA")
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .IsRequired();

                entity.Property(e => e.Estado)
                    .HasColumnName("ESTADO")
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .IsRequired();

                entity.Property(e => e.Mensaje)
                    .HasColumnName("MENSAJE")
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(e => e.FhValidacion)
                    .HasColumnName("FH_VALIDACION")
                    .HasDefaultValueSql("SYSDATETIME()");

                // Relación con DOCUMENTO_PROCESO
                entity.HasOne<DocumentoProceso>()
                    .WithMany()
                    .HasForeignKey(e => e.IdDocumentoProceso)
                    .OnDelete(DeleteBehavior.NoAction);

                // Una regla solo puede tener un resultado vigente
                // por documento.
                entity.HasIndex(e => new
                {
                    e.IdDocumentoProceso,
                    e.CodigoRegla
                }).IsUnique();
            });
        }
    }
}