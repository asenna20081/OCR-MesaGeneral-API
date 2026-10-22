using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces.Validators
{
    public interface IValidadorPolizaRcService
    {
        List<ResultadoValidacionDto> Validar( Dictionary<string, string?> datosPoliza, Dictionary<string, string?> datosPago);
    }
}
