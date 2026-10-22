namespace DocumentosOCRApi.Models
{
    public class CatalogoTipoDocumento
    {
        public int IdTipoDocumento { get; set; }
        public int? ClaveDocumento { get; set; }

        public string NbDocumento { get; set; } = string.Empty;

        public bool RequiereOcr { get; set; }

        public bool StActivo { get; set; }

        public DateTime FhRegistro { get; set; }
    }

    public class Proceso
    {
        public int IdProceso { get; set; }
        public int? IdTipoPersona { get; set; }
        public string StProceso { get; set; } = string.Empty;
        public DateTime FhInicio { get; set; }
        public DateTime? FhFin { get; set; }
    }

    public class DocumentoProceso
    {
        public int IdDocumentoProceso { get; set; }

        public int IdProceso { get; set; }

        public int IdTipoDocumento { get; set; }

        public string NbArchivo { get; set; } = string.Empty;

        public string? TipoArchivo { get; set; }

        public long? NuTamanio { get; set; }

        public string? TextoOcr { get; set; }

        public bool StCargado { get; set; }

        public bool StOcrProcesado { get; set; }

        public DateTime FhCarga { get; set; }
    }

    public class DatoExtraido
    {
        public int IdDatoExtraido { get; set; }

        public int IdDocumentoProceso { get; set; }

        public string NbCampo { get; set; } = string.Empty;

        public string? NbValor { get; set; }

        public DateTime FhRegistro { get; set; }
    }

    public enum TipoPersona
    {
        Fisica = 1,
        Moral = 2
    }
}
