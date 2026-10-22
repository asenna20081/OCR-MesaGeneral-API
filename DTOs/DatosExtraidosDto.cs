namespace DocumentosOCRApi.DTOs
{
    public class DatosExtraidosDto
    {
        public string Campo { get; set; } = string.Empty;
        public string? Valor { get; set; }
        public decimal? Confianza { get; set; }
    }
}
