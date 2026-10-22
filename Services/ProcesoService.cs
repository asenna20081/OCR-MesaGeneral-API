using DocumentosOCRApi.Data;
using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Models;
using DocumentosOCRApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocumentosOCRApi.Services
{
    public class ProcesoService : IProcesoService
    {
        private readonly DocumentosOcrDbContext _context;

        public ProcesoService(DocumentosOcrDbContext context)
        {
            _context = context;
        }


        public async Task<ValidacionDocumentosDto> ValidarDocumentosAsync(
            int idProceso)
        {
            // 1. Verificar que exista el proceso.
            var procesoExiste = await _context.Procesos
                .AnyAsync(x => x.IdProceso == idProceso);

            if (!procesoExiste)
            {
                throw new ArgumentException(
                    "El proceso indicado no existe.");
            }

            // 2. Consultar documentos requeridos.
            var documentosRequeridos = await _context.TiposDocumento
                .Where(x => x.StActivo)
                .Select(x => new
                {
                    x.IdTipoDocumento,
                    x.NbDocumento
                })
                .ToListAsync();

            // 3. Consultar documentos cargados.
            var tiposCargados = await _context.DocumentosProceso
                .Where(x =>
                    x.IdProceso == idProceso &&
                    x.StCargado)
                .Select(x => x.IdTipoDocumento)
                .Distinct()
                .ToListAsync();

            // 4. Identificar documentos faltantes.
            var documentosFaltantes = documentosRequeridos
                .Where(x =>
                    !tiposCargados.Contains(x.IdTipoDocumento))
                .Select(x => x.NbDocumento)
                .ToList();

            bool documentosCompletos =
                documentosFaltantes.Count == 0;

            // 5. Consultar validaciones bloqueantes del expediente.
            var validacionesBloqueantes =
                await (
                    from validacion in _context.ValidacionesDocumento
                    join documento in _context.DocumentosProceso
                        on validacion.IdDocumentoProceso
                        equals documento.IdDocumentoProceso
                    where documento.IdProceso == idProceso
                        && documento.StCargado
                        && (
                            validacion.Estado == "INCONSISTENTE" ||
                            validacion.Estado == "REQUIERE_REVISION"
                        )
                    select new ValidacionBloqueanteDto
                    {
                        IdTipoDocumento = documento.IdTipoDocumento,
                        CodigoRegla = validacion.CodigoRegla,
                        Estado = validacion.Estado,
                        Mensaje = validacion.Mensaje
                    }
                ).ToListAsync();

            // 6. Evaluar las validaciones.
            bool validacionesCorrectas =
                validacionesBloqueantes.Count == 0;

            // 7. El expediente solo puede avanzar si cumple ambas condiciones.
            bool puedeContinuar =
                documentosCompletos && validacionesCorrectas;

            return new ValidacionDocumentosDto
            {
                Completo = documentosCompletos,

                TotalRequeridos = documentosRequeridos.Count,

                TotalCargados = documentosRequeridos.Count -
                                documentosFaltantes.Count,

                DocumentosFaltantes = documentosFaltantes,

                ValidacionesCorrectas = validacionesCorrectas,

                PuedeContinuar = puedeContinuar,

                ValidacionesBloqueantes = validacionesBloqueantes
            };
        }


        public async Task<int> CrearProcesoAsync(int idTipoPersona)
        {
            if (idTipoPersona != 1 && idTipoPersona != 2)
            {
                throw new ArgumentException(
                    "El tipo de persona no es válido.");
            }

            var proceso = new Proceso
            {
                StProceso = "INICIADO",
                IdTipoPersona = idTipoPersona,
                FhInicio = DateTime.Now
            };

            _context.Procesos.Add(proceso);

            await _context.SaveChangesAsync();

            return proceso.IdProceso;
        }

        public async Task<ResumenProcesoDto> ObtenerResumenAsync(int idProceso)
        {
            // 1. Verificar que exista el proceso.
            var proceso = await _context.Procesos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdProceso == idProceso);

            if (proceso == null)
            {
                throw new ArgumentException(
                    "El proceso indicado no existe.");
            }

            // 2. Validar documentos requeridos.
            var validacion = await ValidarDocumentosAsync(idProceso);

            // 3. Recuperar documentos del proceso.
            var documentosProceso = await (
                from documento in _context.DocumentosProceso

                join tipo in _context.TiposDocumento
                    on documento.IdTipoDocumento
                    equals tipo.IdTipoDocumento

                where documento.IdProceso == idProceso

                select new
                {
                    documento.IdDocumentoProceso,
                    documento.IdTipoDocumento,
                    TipoDocumento = tipo.NbDocumento,
                    NombreArchivo = documento.NbArchivo,
                    documento.StCargado
                }
            ).ToListAsync();

            // 4. Recuperar los identificadores de documentos.
            var idsDocumentos = documentosProceso
                .Select(x => x.IdDocumentoProceso)
                .ToList();

            // 5. Recuperar los datos extraídos.
            var datosExtraidos = await _context.DatosExtraidos
                .Where(x => idsDocumentos.Contains(x.IdDocumentoProceso))
                .Select(x => new
                {
                    x.IdDocumentoProceso,
                    x.NbCampo,
                    x.NbValor
                })
                .ToListAsync();

            // 6. Recuperar las validaciones guardadas.
            var validacionesDocumento = await _context.ValidacionesDocumento
                .Where(x => idsDocumentos.Contains(x.IdDocumentoProceso))
                .Select(x => new
                {
                    x.IdDocumentoProceso,
                    x.CodigoRegla,
                    x.Estado,
                    x.Mensaje
                })
                .ToListAsync();

            // 7. Construir el resumen de cada documento.
            var documentos = documentosProceso
                .Select(documento => new DocumentoResumenDto
                {
                    IdTipoDocumento = documento.IdTipoDocumento,
                    TipoDocumento = documento.TipoDocumento,
                    NombreArchivo = documento.NombreArchivo,
                    StCargado = documento.StCargado,

                    DatosExtraidos = datosExtraidos
                        .Where(x =>
                            x.IdDocumentoProceso == documento.IdDocumentoProceso)
                        .GroupBy(x => x.NbCampo)
                        .ToDictionary(
                            grupo => grupo.Key,
                            grupo => grupo.Last().NbValor),

                    Validaciones = validacionesDocumento
                        .Where(x =>
                            x.IdDocumentoProceso == documento.IdDocumentoProceso)
                        .Select(x => new ResultadoValidacionDto
                        {
                            CodigoRegla = x.CodigoRegla,
                            Estado = x.Estado,
                            Mensaje = x.Mensaje
                        })
                        .ToList()
                })
                .ToList();

            // 8. Devolver el resumen.
            return new ResumenProcesoDto
            {
                IdProceso = idProceso,
                IdTipoPersona = proceso.IdTipoPersona,
                DocumentosCompletos = validacion.Completo,
                Documentos = documentos
            };
        }
    }
}
