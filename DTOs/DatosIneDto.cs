namespace DocumentosOCRApi.DTOs
{
    public class DatosIneDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public string Curp { get; set; } = string.Empty;
        public string FechaNacimiento { get; set; } = string.Empty;
        public int? Vigencia { get; set; }
    }
}
