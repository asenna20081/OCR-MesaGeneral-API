namespace DocumentosOCRApi.DTOs
{
    public class DatosIdentidadDto
    {
        public string? Nombre { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }

        public string? RazonSocial { get; set; }

        public string? Curp { get; set; }
        public string? Rfc { get; set; }

        public DateOnly? FechaNacimiento { get; set; }
        public string? Sexo { get; set; }
    }
}
