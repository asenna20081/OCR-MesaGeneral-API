using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces
{
    public interface IExtractorPagoPolizaRcService
    {
        DatosPagoPolizaRcDto ExtraerDatos(string textoOcr);
    }
}
