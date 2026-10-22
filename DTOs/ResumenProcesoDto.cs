namespace DocumentosOCRApi.DTOs
{
    public class ResumenProcesoDto
    {
        public int IdProceso { get; set; }
        public int? IdTipoPersona { get; set; }

        public bool DocumentosCompletos { get; set; }

        public List<DocumentoResumenDto> Documentos { get; set; }
           = new();
    }
}
