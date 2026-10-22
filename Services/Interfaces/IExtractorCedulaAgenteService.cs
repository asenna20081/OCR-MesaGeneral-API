using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces
{
    public interface IExtractorCedulaAgenteService
    {
        DatosCedulaAgenteDto ExtraerDatos(string textoOcr);
    }
}
