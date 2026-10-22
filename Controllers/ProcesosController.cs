using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DocumentosOCRApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcesosController : ControllerBase
    {
        private readonly IProcesoService _procesoService;

        public ProcesosController(IProcesoService procesoService)
        {
            _procesoService = procesoService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearProceso([FromBody] CrearProcesoDto request)
        {
            var idProceso =
                await _procesoService.CrearProcesoAsync(request.IdTipoPersona!.Value);

            return Ok(new
            {
                idProceso = idProceso,
                mensaje = "Proceso creado correctamente",
            });
        }

        [HttpGet("{idProceso}/validar-documentos")]
        public async Task<IActionResult> ValidarDocumentos(
            int idProceso)
        {
            try
            {
                var resultado =
                    await _procesoService.ValidarDocumentosAsync(
                        idProceso);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{idProceso}/resumen")]
        public async Task<IActionResult> ObtenerResumen(
    int idProceso)
        {
            try
            {
                var resultado =
                    await _procesoService.ObtenerResumenAsync(
                        idProceso);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
