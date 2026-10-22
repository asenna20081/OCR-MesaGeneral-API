namespace DocumentosOCRApi.DTOs
{
    public class ResultadoExtraccionDto
    {
        public int IdTipoDocumento { get; set; }
        public int? ClaveDocumento { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;

        public List<DatosExtraidosDto> Datos { get; set; } = new();
    }
}
