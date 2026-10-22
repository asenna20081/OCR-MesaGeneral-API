namespace DocumentosOCRApi.DTOs
{
    public class ResultadoValidacionTipoDocumentoDto
    {
        public bool EsValido { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;
    }
}
