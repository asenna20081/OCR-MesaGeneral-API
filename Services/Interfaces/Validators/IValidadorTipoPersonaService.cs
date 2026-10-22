using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces.Validators
{
    public interface IValidadorTipoPersonaService
    {
        ResultadoValidacionDto Validar( int? idTipoPersona, Dictionary<string, string?> datosExtraidos);
    }
}
