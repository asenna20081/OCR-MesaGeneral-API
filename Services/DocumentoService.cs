using DocumentosOCRApi.Data;
using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Models;
using DocumentosOCRApi.Services.Implementations;
using DocumentosOCRApi.Services.Interfaces;
using DocumentosOCRApi.Services.Interfaces.Validators;
using Microsoft.EntityFrameworkCore;

namespace DocumentosOCRApi.Services
{
    public class DocumentoService : IDocumentoService
    {
        private readonly IOcrService _ocrService;
        private readonly IExtractorIneService _extractorIneService;
        private readonly IExtractorActaService _extractorActaService;
        private readonly IExtractorConstanciaFiscalService _extractorConstanciaFiscal;
        private readonly IExtractorCurpService _extractorCurp;
        private readonly IExtractorComprobanteDomicilioService _extractorComprobanteDomicilioService;
        private readonly IExtractorCedulaAgenteService _extractorCedulaAgenteService;
        private readonly IExtractorEstadoCuentaService _extractorEstadoCuentaService;
        private readonly IExtractorPolizaRcService _extractorPolizaRcService;
        private readonly IExtractorPagoPolizaRcService _extractorPagoPolizaRcService;
        private readonly DocumentosOcrDbContext _context;
        private readonly IValidadorTipoPersonaService _validadorTipoPersona;
        private readonly IValidadorCurpService _validadorCurp;
        private readonly IValidadorIdentidadIneService _validadorIdentidadIne;
        private readonly IValidadorFormatoDocumentoService _validadorFormatoDocumento;
        private readonly IValidadorVigenciaIneService _validadorVigenciaIne;
        private readonly IValidadorPolizaRcService _validadorPolizaRcService;


        public DocumentoService(IOcrService ocrService, IExtractorIneService extractorIneService, IExtractorActaService extractorActaService, IExtractorConstanciaFiscalService extractorConstanciaFiscal, DocumentosOcrDbContext context, IValidadorTipoPersonaService validadorTipoPersona, IExtractorCurpService extractorCurp, IValidadorCurpService validadorCurp, IValidadorIdentidadIneService validadorIdentidadIne, IExtractorComprobanteDomicilioService extractorComprobanteDomicilioService, IValidadorFormatoDocumentoService validadorFormatoDocumento, IValidadorVigenciaIneService validadorVigenciaIne, 
                                IExtractorCedulaAgenteService extractorCedulaAgenteService, IExtractorEstadoCuentaService extractorEstadoCuentaService, IExtractorPolizaRcService extractorPolizaRcService, IExtractorPagoPolizaRcService extractorPagoPolizaRcService, IValidadorPolizaRcService validadorPolizaRcService)
        {
            _ocrService = ocrService;
            _extractorIneService = extractorIneService;
            _extractorActaService = extractorActaService;
            _extractorConstanciaFiscal = extractorConstanciaFiscal;
            _extractorCurp = extractorCurp;
            _extractorComprobanteDomicilioService = extractorComprobanteDomicilioService;
            _extractorCedulaAgenteService = extractorCedulaAgenteService;
            _extractorEstadoCuentaService = extractorEstadoCuentaService;
            _extractorPolizaRcService = extractorPolizaRcService;
            _extractorPagoPolizaRcService = extractorPagoPolizaRcService;
            _context = context;
            _validadorTipoPersona = validadorTipoPersona;
            _validadorCurp = validadorCurp;
            _validadorIdentidadIne = validadorIdentidadIne;
            _validadorFormatoDocumento = validadorFormatoDocumento;
            _validadorVigenciaIne = validadorVigenciaIne;
            _validadorPolizaRcService = validadorPolizaRcService;
        }

        public async Task<DocumentoProcesadoDto> ProcesarDocumentoAsync(ProcesarDocumentoRequestDto request)
        {
            // 1. Validar que exista el proceso
            var proceso = await _context.Procesos.FirstOrDefaultAsync(x => x.IdProceso == request.IdProceso);

            if (proceso == null)
            {
                throw new ArgumentException("El proceso indicado no existe.");
            }

            // 2. Obtener el tipo de documento desde la BD
            var tipoDocumento = await _context.TiposDocumento
                .FirstOrDefaultAsync(x =>
                    x.IdTipoDocumento == request.IdTipoDocumento &&
                    x.StActivo);

            if (tipoDocumento == null)
            {
                throw new ArgumentException( "El tipo de documento indicado no existe o está inactivo.");
            }

            // 3. Validar formato del archivo
            var tiposPermitidos = new[]
            {
                "image/jpeg",
                "image/png",
                "application/pdf"
            };

            if (!tiposPermitidos.Contains(request.Archivo.ContentType))
            {
                throw new ArgumentException( "Formato no permitido. Solo se aceptan archivos JPG, PNG o PDF.");
            }

            // Resultado del OCR
            string textoExtraido = string.Empty;

            // Por ahora lo conservamos porque nuestra respuesta
            // todavía contiene DatosIneDto.
            DatosIneDto? datosIne = null;

            // Aquí almacenaremos cualquier dato extraído,
            // independientemente del tipo de documento.
            var datosExtraidos = new Dictionary<string, string?>();

            //validar tipo de persona
            var validaciones = new List<ResultadoValidacionDto>();

            // 4. Revisar si este tipo de documento requiere OCR
            if (tipoDocumento.RequiereOcr)
            {
                // Una sola llamada a Google Document AI
                var documentoOcr = await _ocrService.ProcesarDocumentoAsync(request.Archivo);

                // Recuperamos el texto plano
                textoExtraido = documentoOcr.Text;

                // Validar que el archivo corresponda al tipo de documento solicitado.
                var validacionFormato = _validadorFormatoDocumento.Validar(tipoDocumento.ClaveDocumento,textoExtraido
                );

                if (!validacionFormato.EsValido)
                {
                    throw new ArgumentException(validacionFormato.Mensaje);
                }

                if (tipoDocumento.ClaveDocumento == 1444)
                {
                    datosIne = _extractorIneService.ExtraerDatos(textoExtraido);
                }
                else if (tipoDocumento.IdTipoDocumento == 3)
                {
                    datosIne = _extractorActaService.ExtraerDatos(documentoOcr);

                } else if (tipoDocumento.ClaveDocumento == 1441)
                {
                    var resultado = _extractorConstanciaFiscal.ExtraerDatos(documentoOcr, tipoDocumento.IdTipoDocumento);

                    datosExtraidos = resultado.Datos
                        .Where(d => !string.IsNullOrWhiteSpace(d.Valor))
                        .GroupBy(d => d.Campo)
                        .ToDictionary(
                            grupo => grupo.Key,
                            grupo => grupo.Last().Valor!
                        );
                }
                else if (tipoDocumento.ClaveDocumento == 1442)
                {
                    var datosCedula =
                        _extractorCedulaAgenteService.ExtraerDatos(textoExtraido);

                    datosExtraidos = new Dictionary<string, string?>
                    {
                        ["numeroCedula"] = datosCedula.NumeroCedula,
                        ["fechaInicioVigencia"] = datosCedula.FechaInicioVigencia,
                        ["fechaFinVigencia"] = datosCedula.FechaFinVigencia,
                        ["rfc"] = datosCedula.Rfc,
                        ["curp"] = datosCedula.Curp,
                        ["nombreCompleto"] = datosCedula.NombreCompleto,
                        ["tipoAutorizacion"] = datosCedula.TipoAutorizacion,
                        ["categoria"] = datosCedula.Categoria
                    };
                } else if (tipoDocumento.ClaveDocumento == 1443) {
                        var resultado = _extractorCurp.ExtraerDatos(documentoOcr, tipoDocumento.IdTipoDocumento);

                        datosExtraidos = resultado.Datos
                            .Where(d => !string.IsNullOrWhiteSpace(d.Valor))
                            .GroupBy(d => d.Campo)
                            .ToDictionary(g => g.Key, g => g.Last().Valor!);
                   }
                else if (tipoDocumento.ClaveDocumento == 1445)
                {
                    var datosEstadoCuenta = _extractorEstadoCuentaService.ExtraerDatos(textoExtraido);

                    datosExtraidos = new Dictionary<string, string?>
                    {
                        ["banco"] = datosEstadoCuenta.Banco,
                        ["titular"] = datosEstadoCuenta.Titular,
                        ["rfc"] = datosEstadoCuenta.Rfc,
                        ["numeroCuenta"] = datosEstadoCuenta.NumeroCuenta,
                        ["clabe"] = datosEstadoCuenta.Clabe,
                        ["fechaCorte"] = datosEstadoCuenta.FechaCorte,
                        ["fechaInicioPeriodo"] = datosEstadoCuenta.FechaInicioPeriodo,
                        ["fechaFinPeriodo"] = datosEstadoCuenta.FechaFinPeriodo,
                        ["moneda"] = datosEstadoCuenta.Moneda
                    };
                }
                else if (tipoDocumento.ClaveDocumento == 1446)
                    {
                    var datosComprobante = _extractorComprobanteDomicilioService.ExtraerDatos( textoExtraido);

                    datosExtraidos = new Dictionary<string, string?>
                    {
                        ["titular"] = datosComprobante.Titular,
                        ["calle"] = datosComprobante.Calle,
                        ["numeroExterior"] = datosComprobante.NumeroExterior,
                        ["numeroInterior"] = datosComprobante.NumeroInterior,
                        ["colonia"] = datosComprobante.Colonia,
                        ["referenciaDomicilio"] = datosComprobante.ReferenciaDomicilio,
                        ["codigoPostal"] = datosComprobante.CodigoPostal,
                        ["municipio"] = datosComprobante.Municipio,
                        ["estado"] = datosComprobante.Estado,
                        ["numeroServicio"] = datosComprobante.NumeroServicio,
                        ["fechaLimitePago"] = datosComprobante.FechaLimitePago
                    };
                    }
                else if (tipoDocumento.ClaveDocumento == 1447)
                {
                    var datosPoliza = _extractorPolizaRcService.ExtraerDatos(textoExtraido);

                    datosExtraidos = new Dictionary<string, string?>
                    {
                        ["aseguradora"] = datosPoliza.Aseguradora,
                        ["numeroPoliza"] = datosPoliza.NumeroPoliza,
                        ["contratante"] = datosPoliza.Contratante,
                        ["rfcContratante"] = datosPoliza.RfcContratante,
                        ["tipoSeguro"] = datosPoliza.TipoSeguro,
                        ["fechaInicioVigencia"] = datosPoliza.FechaInicioVigencia,
                        ["fechaFinVigencia"] = datosPoliza.FechaFinVigencia,
                        ["actividadAsegurada"] = datosPoliza.ActividadAsegurada,
                        ["limiteResponsabilidad"] = datosPoliza.LimiteResponsabilidad,
                        ["importePagar"] = datosPoliza.ImportePagar,
                        ["formaPago"] = datosPoliza.FormaPago,
                        ["moneda"] = datosPoliza.Moneda
                    };
                }
                else if (tipoDocumento.ClaveDocumento == 1448)
                {
                    var datosPago = _extractorPagoPolizaRcService.ExtraerDatos(textoExtraido);

                    datosExtraidos = new Dictionary<string, string?>
                    {
                        ["aseguradora"] = datosPago.Aseguradora,
                        ["numeroPoliza"] = datosPago.NumeroPoliza,
                        ["contratante"] = datosPago.Contratante,
                        ["asegurado"] = datosPago.Asegurado,
                        ["ramo"] = datosPago.Ramo,
                        ["importePagado"] = datosPago.ImportePagado,
                        ["fechaPago"] = datosPago.FechaPago,
                        ["numeroRecibo"] = datosPago.NumeroRecibo,
                        ["estatusRecibo"] = datosPago.EstatusRecibo,
                        ["numeroCobro"] = datosPago.NumeroCobro,
                        ["estatusPoliza"] = datosPago.EstatusPoliza,
                        ["formaPago"] = datosPago.FormaPago,
                        ["fechaInicioPeriodo"] = datosPago.FechaInicioPeriodo,
                        ["fechaFinPeriodo"] = datosPago.FechaFinPeriodo,
                        ["conductoCobro"] = datosPago.ConductoCobro
                    };
                }

                if (datosIne != null)
                {
                    datosExtraidos = new Dictionary<string, string?>
                    {
                        ["nombre"] = datosIne.Nombre,
                        ["apellidoPaterno"] = datosIne.ApellidoPaterno,
                        ["apellidoMaterno"] = datosIne.ApellidoMaterno,
                        ["sexo"] = datosIne.Sexo,
                        ["curp"] = datosIne.Curp,
                        ["fechaNacimiento"] = datosIne.FechaNacimiento,
                        ["vigencia"] = datosIne.Vigencia?.ToString()
                    };

                    var validacionVigencia = _validadorVigenciaIne.Validar(datosIne.Vigencia);
                    validaciones.Add(validacionVigencia);
                }

                // Validación específica para la constancia fiscal.
                if (tipoDocumento.ClaveDocumento == 1441)
                {
                    var validacion = _validadorTipoPersona.Validar(proceso.IdTipoPersona, datosExtraidos );
                    validaciones.Add(validacion);
                }
            }

            // 5. Registrar el documento cargado
            var documentoProceso =
                await _context.DocumentosProceso .FirstOrDefaultAsync(x =>
                        x.IdProceso == request.IdProceso &&
                        x.IdTipoDocumento == request.IdTipoDocumento);

            if (documentoProceso == null)
            {
                // Primera vez que se carga este tipo de documento.
                documentoProceso = new DocumentoProceso
                {
                    IdProceso = request.IdProceso,
                    IdTipoDocumento = request.IdTipoDocumento,
                    NbArchivo = request.Archivo.FileName,
                    TipoArchivo = request.Archivo.ContentType,
                    NuTamanio = request.Archivo.Length,
                    TextoOcr = tipoDocumento.RequiereOcr ? textoExtraido : null,
                    StCargado = true,
                    StOcrProcesado = tipoDocumento.RequiereOcr,
                    FhCarga = DateTime.Now
                };

                _context.DocumentosProceso.Add(documentoProceso);

                await _context.SaveChangesAsync();
            }
            else
            {
                // Ya existe: el usuario está reemplazando el documento.
                documentoProceso.NbArchivo = request.Archivo.FileName;
                documentoProceso.TipoArchivo = request.Archivo.ContentType;
                documentoProceso.NuTamanio = request.Archivo.Length;

                documentoProceso.TextoOcr = tipoDocumento.RequiereOcr ? textoExtraido : null;

                documentoProceso.StCargado = true;

                documentoProceso.StOcrProcesado = tipoDocumento.RequiereOcr;

                documentoProceso.FhCarga = DateTime.Now;

                // Eliminamos los datos que pertenecían
                // a la versión anterior del documento.
                var datosAnteriores =
                    await _context.DatosExtraidos
                        .Where(x =>
                            x.IdDocumentoProceso ==
                            documentoProceso.IdDocumentoProceso)
                        .ToListAsync();

                _context.DatosExtraidos.RemoveRange(
                    datosAnteriores);

                await _context.SaveChangesAsync();
            }

            // En este punto EF ya recuperó el IDENTITY generado
            // y tenemos documentoProceso.IdDocumentoProceso.

            // 6. Guardar los campos encontrados
            await GuardarDatosExtraidosAsync( documentoProceso.IdDocumentoProceso, datosExtraidos);

            //Guardar las validaciones sobre el documento procesado
            await GuardarValidacionesAsync( documentoProceso.IdDocumentoProceso, validaciones);

            //Validar la CURP de RENAPO y de la constancia fiscal
            if (tipoDocumento.ClaveDocumento == 1441 || tipoDocumento.ClaveDocumento == 1443)
            {
                await RecalcularValidacionCurpAsync( request.IdProceso, proceso.IdTipoPersona);
            }

            if (tipoDocumento.ClaveDocumento == 1443 || tipoDocumento.ClaveDocumento == 1444)
            {
                await RecalcularValidacionIdentidadIneAsync( request.IdProceso,  proceso.IdTipoPersona);
            }

            // Validación cruzada entre póliza RC y comprobante de pago.
            if (tipoDocumento.ClaveDocumento == 1447 || tipoDocumento.ClaveDocumento == 1448)
            {
                await RecalcularValidacionPolizaRcAsync( request.IdProceso);
            }


            // 7. Regresar resultado
            return new DocumentoProcesadoDto
            {
                Mensaje = tipoDocumento.RequiereOcr
                    ? "Documento cargado y procesado correctamente"
                    : "Documento cargado correctamente",

                NombreArchivo = request.Archivo.FileName,
                TipoArchivo = request.Archivo.ContentType,
                Tamanio = request.Archivo.Length,

                TextoExtraido = textoExtraido,

                Datos = datosExtraidos,
                Validaciones = validaciones
            };
        }

        private async Task GuardarDatosExtraidosAsync(
            int idDocumentoProceso,
            Dictionary<string, string?> datos)
        {
            // Si el documento no produjo información,
            // no tenemos nada que insertar.
            if (datos.Count == 0)
            {
                return;
            }

            var fechaRegistro = DateTime.Now;

            var registros = datos
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => new DatoExtraido
                {
                    IdDocumentoProceso =
                        idDocumentoProceso,

                    NbCampo = x.Key,

                    NbValor = x.Value,

                    FhRegistro = fechaRegistro
                })
                .ToList();

            if (registros.Count == 0)
            {
                return;
            }

            _context.DatosExtraidos.AddRange(registros);

            await _context.SaveChangesAsync();
        }

        private async Task GuardarValidacionesAsync( int idDocumentoProceso, List<ResultadoValidacionDto> validaciones)
        {
            // Recuperar las validaciones anteriores del documento.
            var validacionesAnteriores = await _context.ValidacionesDocumento
                .Where(x => x.IdDocumentoProceso == idDocumentoProceso)
                .ToListAsync();

            // Eliminar los resultados anteriores.
            _context.ValidacionesDocumento.RemoveRange(validacionesAnteriores);

            // Registrar los resultados actuales.
            foreach (var validacion in validaciones)
            {
                var nuevaValidacion = new ValidacionDocumento
                {
                    IdDocumentoProceso = idDocumentoProceso,
                    CodigoRegla = validacion.CodigoRegla,
                    Estado = validacion.Estado,
                    Mensaje = validacion.Mensaje,
                    FhValidacion = DateTime.Now
                };

                _context.ValidacionesDocumento.Add(nuevaValidacion);
            }

            await _context.SaveChangesAsync();
        }

        private async Task RecalcularValidacionCurpAsync( int idProceso,int? idTipoPersona)
        {
            const string codigoRegla = "VAL_IDENTIDAD_002";

            // La comparación aplica únicamente a personas físicas.
            if (idTipoPersona != 1)
                return;

            // Recuperar las constancias fiscal y CURP del expediente.
            var documentos = await (
                from documento in _context.DocumentosProceso

                join tipo in _context.TiposDocumento
                    on documento.IdTipoDocumento
                    equals tipo.IdTipoDocumento

                where documento.IdProceso == idProceso
                    && (tipo.ClaveDocumento == 1441 ||
                        tipo.ClaveDocumento == 1443)

                select new
                {
                    documento.IdDocumentoProceso,
                    tipo.ClaveDocumento
                }
            ).ToListAsync();

            // Asociamos esta regla a la constancia CURP.
            var documentoCurp = documentos
                .FirstOrDefault(x => x.ClaveDocumento == 1443);

            if (documentoCurp == null)
                return;

            // Eliminar el resultado anterior de esta regla.
            var validacionesAnteriores =
                await _context.ValidacionesDocumento
                    .Where(x =>
                        x.IdDocumentoProceso ==
                            documentoCurp.IdDocumentoProceso &&
                        x.CodigoRegla == codigoRegla)
                    .ToListAsync();

            _context.ValidacionesDocumento
                .RemoveRange(validacionesAnteriores);

            // Obtener la CURP de cada documento.
            var idsDocumentos = documentos
                .Select(x => x.IdDocumentoProceso)
                .ToList();

            var datosCurp = await _context.DatosExtraidos
                .Where(x =>
                    idsDocumentos.Contains(x.IdDocumentoProceso) &&
                    x.NbCampo == "curp")
                .Select(x => new
                {
                    x.IdDocumentoProceso,
                    x.NbValor
                })
                .ToListAsync();

            var documentoFiscal = documentos
                .FirstOrDefault(x => x.ClaveDocumento == 1441);

            var curpFiscal = datosCurp
                .FirstOrDefault(x =>
                    x.IdDocumentoProceso ==
                        documentoFiscal?.IdDocumentoProceso)
                ?.NbValor;

            var curpRenapo = datosCurp
                .FirstOrDefault(x =>
                    x.IdDocumentoProceso ==
                        documentoCurp.IdDocumentoProceso)
                ?.NbValor;

            // Ejecutar la regla de comparación.
            var resultado = _validadorCurp.Validar(
                curpFiscal,
                curpRenapo);

            _context.ValidacionesDocumento.Add(
                new ValidacionDocumento
                {
                    IdDocumentoProceso =
                        documentoCurp.IdDocumentoProceso,

                    CodigoRegla = resultado.CodigoRegla,
                    Estado = resultado.Estado,
                    Mensaje = resultado.Mensaje,
                    FhValidacion = DateTime.Now
                });

            await _context.SaveChangesAsync();
        }


        private async Task RecalcularValidacionPolizaRcAsync(int idProceso)
        {
            var codigosReglas = new[]
            {
                "VAL_POLIZA_RC_001",
                "VAL_POLIZA_RC_002",
                "VAL_POLIZA_RC_003"
            };

            // 1. Recuperar la póliza y su comprobante de pago.
            var documentos = await (
                from documento in _context.DocumentosProceso
                join tipo in _context.TiposDocumento
                    on documento.IdTipoDocumento equals tipo.IdTipoDocumento
                where documento.IdProceso == idProceso
                    && documento.StCargado
                    && (tipo.ClaveDocumento == 1447 ||
                        tipo.ClaveDocumento == 1448)
                select new
                {
                    documento.IdDocumentoProceso,
                    tipo.ClaveDocumento
                }
            ).ToListAsync();

            var documentoPoliza = documentos
                .FirstOrDefault(x => x.ClaveDocumento == 1447);

            var documentoPago = documentos
                .FirstOrDefault(x => x.ClaveDocumento == 1448);

            // Asociamos las validaciones al comprobante de pago.
            // Si todavía no existe, esperamos a que sea cargado.
            if (documentoPago == null)
                return;

            // 2. Eliminar únicamente las tres reglas de póliza RC.
            var validacionesAnteriores =
                await _context.ValidacionesDocumento
                    .Where(x =>
                        x.IdDocumentoProceso ==
                            documentoPago.IdDocumentoProceso &&
                        codigosReglas.Contains(x.CodigoRegla))
                    .ToListAsync();

            _context.ValidacionesDocumento.RemoveRange(
                validacionesAnteriores);

            // 3. Recuperar los datos extraídos de ambos documentos.
            var idsDocumentos = documentos
                .Select(x => x.IdDocumentoProceso)
                .ToList();

            var datos = await _context.DatosExtraidos
                .Where(x =>
                    idsDocumentos.Contains(x.IdDocumentoProceso))
                .Select(x => new
                {
                    x.IdDocumentoProceso,
                    x.NbCampo,
                    x.NbValor
                })
                .ToListAsync();

            // 4. Construir diccionarios para el validador.
            var datosPoliza = documentoPoliza == null
                ? new Dictionary<string, string?>()
                : datos
                    .Where(x =>
                        x.IdDocumentoProceso ==
                            documentoPoliza.IdDocumentoProceso)
                    .GroupBy(x => x.NbCampo)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Last().NbValor);

            var datosPago = datos
                .Where(x =>
                    x.IdDocumentoProceso ==
                        documentoPago.IdDocumentoProceso)
                .GroupBy(x => x.NbCampo)
                .ToDictionary(
                    x => x.Key,
                    x => x.Last().NbValor);

            // 5. Ejecutar las tres reglas.
            var resultados = _validadorPolizaRcService.Validar(
                datosPoliza,
                datosPago);

            // 6. Guardar los resultados actualizados.
            foreach (var resultado in resultados)
            {
                _context.ValidacionesDocumento.Add(
                    new ValidacionDocumento
                    {
                        IdDocumentoProceso =
                            documentoPago.IdDocumentoProceso,

                        CodigoRegla = resultado.CodigoRegla,
                        Estado = resultado.Estado,
                        Mensaje = resultado.Mensaje,
                        FhValidacion = DateTime.Now
                    });
            }

            await _context.SaveChangesAsync();
        }


        private async Task RecalcularValidacionIdentidadIneAsync( int idProceso, int? idTipoPersona)
        {
            // Esta validación solo aplica a personas físicas.
            if (idTipoPersona != 1)
                return;

            var documentos = await (
                from documento in _context.DocumentosProceso
                join tipo in _context.TiposDocumento
                    on documento.IdTipoDocumento equals tipo.IdTipoDocumento
                where documento.IdProceso == idProceso
                      && (tipo.ClaveDocumento == 1443 ||
                          tipo.ClaveDocumento == 1444)
                select new
                {
                    documento.IdDocumentoProceso,
                    tipo.ClaveDocumento
                }
            ).ToListAsync();

            var documentoIne = documentos.FirstOrDefault(
                x => x.ClaveDocumento == 1444);

            var documentoCurp = documentos.FirstOrDefault(
                x => x.ClaveDocumento == 1443);

            // Si todavía no existe la INE, no hay un documento
            // donde registrar el resultado.
            if (documentoIne == null)
                return;

            var idsDocumentos = documentos
                .Select(x => x.IdDocumentoProceso)
                .ToList();

            var datosCurp = await _context.DatosExtraidos
                .Where(x =>
                    idsDocumentos.Contains(x.IdDocumentoProceso) &&
                    x.NbCampo == "curp")
                .Select(x => new
                {
                    x.IdDocumentoProceso,
                    x.NbValor
                })
                .ToListAsync();

            var curpIne = datosCurp.FirstOrDefault(
                x => x.IdDocumentoProceso ==
                     documentoIne.IdDocumentoProceso)?.NbValor;

            var curpConstancia = documentoCurp == null
                ? null
                : datosCurp.FirstOrDefault(
                    x => x.IdDocumentoProceso ==
                         documentoCurp.IdDocumentoProceso)?.NbValor;

            var resultado = _validadorIdentidadIne.Validar(
                curpIne,
                curpConstancia);

            // Eliminar el resultado anterior para evitar duplicados.
            var validacionAnterior = await _context.ValidacionesDocumento
                .FirstOrDefaultAsync(x =>
                    x.IdDocumentoProceso ==
                        documentoIne.IdDocumentoProceso &&
                    x.CodigoRegla == "VAL_IDENTIDAD_003");

            if (validacionAnterior != null)
            {
                _context.ValidacionesDocumento.Remove(
                    validacionAnterior);

                await _context.SaveChangesAsync();
            }

            _context.ValidacionesDocumento.Add(
                new ValidacionDocumento
                {
                    IdDocumentoProceso =
                        documentoIne.IdDocumentoProceso,
                    CodigoRegla = resultado.CodigoRegla,
                    Estado = resultado.Estado,
                    Mensaje = resultado.Mensaje
                });

            await _context.SaveChangesAsync();
        }
    }
}
