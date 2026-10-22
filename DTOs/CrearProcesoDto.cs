using System.ComponentModel.DataAnnotations;

namespace DocumentosOCRApi.DTOs
{
    public class CrearProcesoDto
    {
        [Required]
        [Range(1, 2, ErrorMessage =
            "El tipo de persona debe ser 1 (Física) o 2 (Moral).")]
        public int? IdTipoPersona { get; set; }
    }
}
