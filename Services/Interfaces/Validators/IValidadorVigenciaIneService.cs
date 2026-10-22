using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces.Validators
{
    public interface IValidadorVigenciaIneService
    {
        ResultadoValidacionDto Validar(int? anioVigencia);
    }
}
