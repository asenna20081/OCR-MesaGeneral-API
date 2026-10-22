using DocumentosOCRApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentosOCRApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposDocumentoController : ControllerBase
    {
        private readonly DocumentosOcrDbContext _context;

        public TiposDocumentoController(DocumentosOcrDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTiposDocumento()
        {
            var documentos = await _context.TiposDocumento
                .Where(x => x.StActivo)
                .Select(x => new
                {
                    idTipoDocumento = x.IdTipoDocumento,
                    ClaveDocumento = x.ClaveDocumento,
                    nombre = x.NbDocumento,
                    requiereOcr = x.RequiereOcr
                })
                .ToListAsync();

            return Ok(documentos);
        }
    }
}