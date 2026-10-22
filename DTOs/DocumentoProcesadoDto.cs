namespace DocumentosOCRApi.DTOs
{
    public class DocumentoProcesadoDto
    {
        public string Mensaje { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty;
        public long Tamanio { get; set; }
        public string TextoExtraido { get; set; } = string.Empty;
        public Dictionary<string, string?> Datos { get; set; } = new();
        public List<ResultadoValidacionDto> Validaciones { get; set; } = new();
    }

    public class ProcesarDocumentoRequestDto
    {
        public int IdProceso { get; set; }

        public int IdTipoDocumento { get; set; }

        public IFormFile Archivo { get; set; } = null!;
    }
}
