namespace DocumentosOCRApi.DTOs
{
    public class DatosComprobanteDomicilioDto
    {
        public string? Titular { get; set; }
        public string? Calle { get; set; }
        public string? NumeroExterior { get; set; }
        public string? NumeroInterior { get; set; }
        public string? Colonia { get; set; }
        public string? ReferenciaDomicilio { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Municipio { get; set; }
        public string? Estado { get; set; }
        public string? NumeroServicio { get; set; }
        public string? FechaLimitePago { get; set; }
    }
}
