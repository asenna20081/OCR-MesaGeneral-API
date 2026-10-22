namespace DocumentosOCRApi.DTOs
{
    public class ValidacionDocumentosDto
    {
        public bool Completo { get; set; }

        public int TotalRequeridos { get; set; }

        public int TotalCargados { get; set; }

        public List<string> DocumentosFaltantes { get; set; } = new();
        public bool ValidacionesCorrectas { get; set; }

        public bool PuedeContinuar { get; set; }

        public List<ValidacionBloqueanteDto> ValidacionesBloqueantes { get; set; } = new();
    }

    public class ValidacionBloqueanteDto
    {
        public int IdTipoDocumento { get; set; }

        public string CodigoRegla { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;
    }
}
