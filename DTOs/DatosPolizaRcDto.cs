namespace DocumentosOCRApi.DTOs
{
    public class DatosPolizaRcDto
    {
        public string? Aseguradora { get; set; }
        public string? NumeroPoliza { get; set; }
        public string? Contratante { get; set; }
        public string? RfcContratante { get; set; }
        public string? TipoSeguro { get; set; }
        public string? FechaInicioVigencia { get; set; }
        public string? FechaFinVigencia { get; set; }
        public string? ActividadAsegurada { get; set; }
        public string? LimiteResponsabilidad { get; set; }
        public string? ImportePagar { get; set; }
        public string? FormaPago { get; set; }
        public string? Moneda { get; set; }
    }
}
