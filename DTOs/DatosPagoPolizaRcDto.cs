namespace DocumentosOCRApi.DTOs
{
    public class DatosPagoPolizaRcDto
    {
        public string? Aseguradora { get; set; }
        public string? NumeroPoliza { get; set; }
        public string? Contratante { get; set; }
        public string? Asegurado { get; set; }
        public string? Ramo { get; set; }
        public string? ImportePagado { get; set; }
        public string? FechaPago { get; set; }
        public string? NumeroRecibo { get; set; }
        public string? EstatusRecibo { get; set; }
        public string? NumeroCobro { get; set; }
        public string? EstatusPoliza { get; set; }
        public string? FormaPago { get; set; }
        public string? FechaInicioPeriodo { get; set; }
        public string? FechaFinPeriodo { get; set; }
        public string? ConductoCobro { get; set; }
    }
}
