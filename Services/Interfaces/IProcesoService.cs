using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces
{
    public interface IProcesoService
    {
        Task<int> CrearProcesoAsync(int idTipoPersona);
        Task<ValidacionDocumentosDto> ValidarDocumentosAsync(
            int idProceso);
        Task<ResumenProcesoDto> ObtenerResumenAsync(
            int idProceso);
    }
}
