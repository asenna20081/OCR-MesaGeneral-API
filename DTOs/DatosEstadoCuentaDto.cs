namespace DocumentosOCRApi.DTOs
{
    public class DatosEstadoCuentaDto
    {
        public string? Banco { get; set; }
        public string? Titular { get; set; }
        public string? Rfc { get; set; }
        public string? NumeroCuenta { get; set; }
        public string? Clabe { get; set; }
        public string? FechaCorte { get; set; }
        public string? FechaInicioPeriodo { get; set; }
        public string? FechaFinPeriodo { get; set; }
        public string? Moneda { get; set; }
    }
}
