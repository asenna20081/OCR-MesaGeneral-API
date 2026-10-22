using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces
{
    public interface IExtractorPolizaRcService
    {
        DatosPolizaRcDto ExtraerDatos(string textoOcr);
    }
}
