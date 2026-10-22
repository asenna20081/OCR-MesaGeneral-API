using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces.Validators
{
    public interface IValidadorCurpService
    {
        ResultadoValidacionDto Validar( string? curpFiscal, string? curpRenapo);
    }
}
