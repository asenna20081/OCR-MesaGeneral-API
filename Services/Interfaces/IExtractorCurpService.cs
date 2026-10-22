using DocumentosOCRApi.DTOs;
using Google.Cloud.DocumentAI.V1;

namespace DocumentosOCRApi.Services.Interfaces
{
    public interface IExtractorCurpService
    {
        ResultadoExtraccionDto ExtraerDatos( Document documento, int idTipoDocumento);
    }
}
