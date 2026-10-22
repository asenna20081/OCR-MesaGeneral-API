namespace DocumentosOCRApi.Models
{
    public class ValidacionDocumento
    {
        public int IdValidacionDocumento { get; set; }

        public int IdDocumentoProceso { get; set; }

        public string CodigoRegla { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public DateTime FhValidacion { get; set; }
    }
}