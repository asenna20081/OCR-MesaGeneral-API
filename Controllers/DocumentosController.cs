using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocumentosOCRApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentosController : ControllerBase
    {
        private readonly IDocumentoService _documentoService;

        public DocumentosController(IDocumentoService documentoService)
        {
            _documentoService = documentoService;
        }

        [HttpPost("procesar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ProcesarDocumento([FromForm] ProcesarDocumentoRequestDto request)
        {
            if (request.Archivo == null || request.Archivo.Length == 0)
            {
                return BadRequest("No se recibió ningún archivo.");
            }

            try
            {
                var resultado = await _documentoService.ProcesarDocumentoAsync(request);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
