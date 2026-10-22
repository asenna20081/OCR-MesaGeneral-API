using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces.Validators
{
    public interface IValidadorIdentidadIneService
    {
        ResultadoValidacionDto Validar( string? curpIne, string? curpConstancia);
    }
}
