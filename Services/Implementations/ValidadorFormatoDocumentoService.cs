using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces.Validators;
using System.Text;
using System.Text.RegularExpressions;

namespace DocumentosOCRApi.Services.Implementations
{
    public class ValidadorFormatoDocumentoService : IValidadorFormatoDocumentoService
    {
        private static readonly Dictionary<int, string[]> Evidencias =
            new()
            {
                [1441] = new[]
                {
                "SERVICIO DE ADMINISTRACION TRIBUTARIA",
                "CEDULA DE IDENTIFICACION FISCAL",
                "CONSTANCIA DE SITUACION FISCAL",
                "REGISTRO FEDERAL DE CONTRIBUYENTES"
                },

                [1442] = new[]
                {
                    "COMISION NACIONAL DE SEGUROS Y FIANZAS",
                    "AGENTE DE SEGUROS POR CUENTA PROPIA",
                    "REGLAMENTO DE AGENTES DE SEGUROS",
                    "CNSF"
                },

                [1443] = new[]
                {
                "CLAVE UNICA DE REGISTRO DE POBLACION",
                "CONSTANCIA DE LA CLAVE UNICA",
                "CURP CERTIFICADA",
                "RENAPO"
                },

                [1444] = new[]
                {
                "INSTITUTO NACIONAL ELECTORAL",
                "CREDENCIAL PARA VOTAR",
                "CLAVE DE ELECTOR",
                "SECCION",
                "VIGENCIA"
                },

                [1445] = new[]
                {
                    "SCOTIABANK",
                    "ESTADO DE CUENTA",
                    "FECHA DE CORTE",
                    "RESUMEN DE SALDOS"
                },

                [1446] = new[]
                {
                "COMISION FEDERAL DE ELECTRICIDAD",
                "NO DE SERVICIO",
                "PERIODO FACTURADO",
                "RMU"
                },

                [1447] = new[]
                {
                    "SEGURO DE RC PROFESIONAL",
                    "AGENTES DE SEGUROS Y/O DE FIANZAS",
                    "RESUMEN DE LA POLIZA",
                    "LIMITE MAXIMO DE RESPONSABILIDAD"
                },

                [1448] = new[]
                {
                    "DETALLE DE PAGO",
                    "ESTATUS DEL RECIBO",
                    "NO. DE RECIBO",
                    "FECHA DE PAGO"
                },
            };

        public ResultadoValidacionTipoDocumentoDto Validar(
            int? claveDocumento,
            string textoOcr)
        {
            if (!claveDocumento.HasValue ||
                !Evidencias.ContainsKey(claveDocumento.Value))
            {
                return new ResultadoValidacionTipoDocumentoDto
                {
                    EsValido = true,
                    Estado = "VALIDO",
                    Mensaje = "Este tipo de documento todavía no tiene validación de formato configurada."
                };
            }

            if (string.IsNullOrWhiteSpace(textoOcr))
            {
                return new ResultadoValidacionTipoDocumentoDto
                {
                    EsValido = false,
                    Estado = "REQUIERE_REVISION",
                    Mensaje = "No fue posible identificar el contenido del documento."
                };
            }

            var textoNormalizado = Normalizar(textoOcr);

            var evidenciasEsperadas = Evidencias[claveDocumento.Value];

            var coincidenciasEsperadas = evidenciasEsperadas.Count(
                evidencia => textoNormalizado.Contains(
                    Normalizar(evidencia)));

            // Identificar si hay evidencias de otro documento conocido.
            var otroDocumento = Evidencias
                .Where(x => x.Key != claveDocumento.Value)
                .Select(x => new
                {
                    Clave = x.Key,
                    Coincidencias = x.Value.Count(
                        evidencia => textoNormalizado.Contains(
                            Normalizar(evidencia)))
                })
                .OrderByDescending(x => x.Coincidencias)
                .FirstOrDefault();

            if (otroDocumento != null &&
                otroDocumento.Coincidencias >= 2 &&
                otroDocumento.Coincidencias > coincidenciasEsperadas)
            {
                return new ResultadoValidacionTipoDocumentoDto
                {
                    EsValido = false,
                    Estado = "INCONSISTENTE",
                    Mensaje =
                        "El archivo cargado parece corresponder a un tipo de documento diferente al solicitado."
                };
            }

            if (coincidenciasEsperadas >= 2)
            {
                return new ResultadoValidacionTipoDocumentoDto
                {
                    EsValido = true,
                    Estado = "VALIDO",
                    Mensaje = "El formato del documento corresponde al tipo solicitado."
                };
            }

            return new ResultadoValidacionTipoDocumentoDto
            {
                EsValido = false,
                Estado = "REQUIERE_REVISION",
                Mensaje =
                    "No se encontraron suficientes elementos para confirmar que el documento corresponde al tipo solicitado."
            };
        }

        private static string Normalizar(string texto)
        {
            var normalizado = texto
                .ToUpperInvariant()
                .Normalize(NormalizationForm.FormD);

            var sinAcentos = new string(
                normalizado
                    .Where(c =>
                        System.Globalization.CharUnicodeInfo
                            .GetUnicodeCategory(c) !=
                        System.Globalization.UnicodeCategory.NonSpacingMark)
                    .ToArray());

            return Regex.Replace(
                sinAcentos,
                @"[^A-Z0-9]+",
                " ").Trim();
        }
    }
}
