using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces.Validators
{
    public interface IValidadorFormatoDocumentoService
    {
        ResultadoValidacionTipoDocumentoDto Validar(int? claveDocumento, string textoOcr);
    }
}
