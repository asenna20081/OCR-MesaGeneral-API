using DocumentosOCRApi.Services.Interfaces;
using Google.Cloud.DocumentAI.V1;
using Google.Protobuf;

namespace DocumentosOCRApi.Services
{
    public class GoogleDocumentAiOcrService : IOcrService
    {
        private readonly IConfiguration _configuration;

        public GoogleDocumentAiOcrService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> ExtraerTextoAsync(
        IFormFile archivo)
        {
            var documento =
                await ProcesarDocumentoAsync(archivo);

            return documento.Text;
        }

        public async Task<Document> ProcesarDocumentoAsync(
            IFormFile archivo)
        {
            var projectId =
                _configuration["GoogleDocumentAI:ProjectId"];

            var location =
                _configuration["GoogleDocumentAI:Location"];

            var processorId =
                _configuration["GoogleDocumentAI:ProcessorId"];

            var client =
                await new DocumentProcessorServiceClientBuilder
                {
                    Endpoint =
                        $"{location}-documentai.googleapis.com"
                }.BuildAsync();

            using var memoryStream = new MemoryStream();

            await archivo.CopyToAsync(memoryStream);

            var rawDocument = new RawDocument
            {
                Content = ByteString.CopyFrom(
                    memoryStream.ToArray()),

                MimeType = archivo.ContentType
            };

            var processorName =
                ProcessorName.FromProjectLocationProcessor(
                    projectId,
                    location,
                    processorId
                );

            var request = new ProcessRequest
            {
                Name = processorName.ToString(),
                RawDocument = rawDocument
            };

            var response =
                await client.ProcessDocumentAsync(request);

            return response.Document;
        }
    }
}
