using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces
{
    public interface IExtractorComprobanteDomicilioService
    {
        DatosComprobanteDomicilioDto ExtraerDatos(string textoOcr);
    }
}
