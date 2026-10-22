using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces
{
    public interface IDocumentoService
    {
        Task<DocumentoProcesadoDto> ProcesarDocumentoAsync(ProcesarDocumentoRequestDto request);
    }
}
