namespace DocumentosOCRApi.DTOs
{
    public class DocumentoResumenDto
    {
        public int IdTipoDocumento { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public bool StCargado { get; set; }
        public Dictionary<string, string?> DatosExtraidos { get; set; } = new();
        public List<ResultadoValidacionDto> Validaciones { get; set; } = new();
    }
}
