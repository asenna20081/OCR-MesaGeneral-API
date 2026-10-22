using Google.Cloud.DocumentAI.V1;

namespace DocumentosOCRApi.Services.Interfaces
{
    public interface IOcrService
    {
        Task<string> ExtraerTextoAsync(IFormFile archivo);
        Task<Document> ProcesarDocumentoAsync(IFormFile archivo);
    }
}
