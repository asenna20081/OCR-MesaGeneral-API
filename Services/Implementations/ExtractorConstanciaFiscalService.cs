using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces;
using Google.Cloud.DocumentAI.V1;
using System.Text.RegularExpressions;

namespace DocumentosOCRApi.Services.Implementations
{
    public class ExtractorConstanciaFiscalService
       : IExtractorConstanciaFiscalService
    {
        public ResultadoExtraccionDto ExtraerDatos(Document documento, int idTipoDocumento)
        {
            var texto = documento.Text ?? string.Empty;

            var resultado = new ResultadoExtraccionDto
            {
                IdTipoDocumento = idTipoDocumento,
                ClaveDocumento = 1441,
                TipoDocumento = "CONSTANCIA DE SITUACIÓN FISCAL"
            };

            var rfc = Regex.Match(
                texto,
                @"\b[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}\b",
                RegexOptions.IgnoreCase);

            var curp = Regex.Match(
                texto,
                @"\b[A-Z]{4}\d{6}[HM][A-Z]{5}[A-Z0-9]\d\b",
                RegexOptions.IgnoreCase);

            if (rfc.Success)
            {
                var tipoPersona = rfc.Value.Length == 13
                    ? "FISICA"
                    : "MORAL";

                resultado.Datos.Add(new DatosExtraidosDto
                {
                    Campo = "tipoPersona",
                    Valor = tipoPersona
                });
                resultado.Datos.Add(new DatosExtraidosDto
                {
                    Campo = "rfc",
                    Valor = rfc.Value.ToUpperInvariant()
                });
            }

            if (rfc.Success && rfc.Value.Length == 13 && curp.Success)
            {
                resultado.Datos.Add(new DatosExtraidosDto
                {
                    Campo = "curp",
                    Valor = curp.Value.ToUpperInvariant()
                });
            }

            var valores = ObtenerValoresIdentificacion(texto);

            if (rfc.Success && rfc.Value.Length == 13 && valores.Length == 5)
            {
                resultado.Datos.Add(new DatosExtraidosDto
                {
                    Campo = "nombre",
                    Valor = valores[2]
                });

                resultado.Datos.Add(new DatosExtraidosDto
                {
                    Campo = "apellidoPaterno",
                    Valor = valores[3]
                });

                resultado.Datos.Add(new DatosExtraidosDto
                {
                    Campo = "apellidoMaterno",
                    Valor = valores[4]
                });
            }

            if (rfc.Success && rfc.Value.Length == 12)
            {
                var razonSocial = ExtraerRazonSocial(texto);

                if (!string.IsNullOrWhiteSpace(razonSocial))
                {
                    resultado.Datos.Add(new DatosExtraidosDto
                    {
                        Campo = "razonSocial",
                        Valor = razonSocial
                    });
                }
            }

            return resultado;
        }

        private static string ExtraerValorPorPosicion( string texto, string etiquetaInicio, int posicion)
        {
            var lineas = texto
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            int indice = lineas.FindIndex(x =>
                x.Equals(
                    etiquetaInicio,
                    StringComparison.OrdinalIgnoreCase));

            if (indice == -1)
                return string.Empty;

            int indiceValores = indice + 1;

            // Saltamos las etiquetas que aparecen antes de los valores.
            while (indiceValores < lineas.Count &&
                   lineas[indiceValores].EndsWith(":"))
            {
                indiceValores++;
            }

            int indiceResultado = indiceValores + posicion;

            return indiceResultado < lineas.Count
                ? lineas[indiceResultado]
                : string.Empty;
        }

        private static string ExtraerRazonSocial(string texto)
        {
            var lineas = texto
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            // Localizar la sección de identificación fiscal.
            int inicioSeccion = lineas.FindIndex(x =>
                x.Contains(
                    "Datos de Identificación del Contribuyente",
                    StringComparison.OrdinalIgnoreCase));

            if (inicioSeccion == -1)
                return string.Empty;

            // Localizar la etiqueta de razón social.
            int indiceRazonSocial = lineas.FindIndex(
                inicioSeccion + 1,
                x => Regex.IsMatch(
                    x,
                    @"Denominaci[oó]n\s*/\s*Raz[oó]n\s+Social",
                    RegexOptions.IgnoreCase));

            if (indiceRazonSocial == -1)
                return string.Empty;

            // Identificar el final del grupo de etiquetas.
            int indiceUltimaEtiqueta = lineas.FindIndex(
                indiceRazonSocial + 1,
                x => x.Contains(
                    "Fecha de último cambio de estado",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceUltimaEtiqueta == -1)
                return string.Empty;

            // En este formato, el primer valor después del
            // grupo de etiquetas corresponde a la razón social.
            int indiceValor = indiceUltimaEtiqueta + 1;

            if (indiceValor >= lineas.Count)
                return string.Empty;

            var razonSocial = lineas[indiceValor];

            // Evitar devolver otra etiqueta como valor.
            if (razonSocial.EndsWith(":"))
                return string.Empty;

            return razonSocial;
        }

        private static string[] ObtenerValoresIdentificacion( string texto)
        {
            var lineas = texto
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();

            int indice = Array.FindIndex(
                lineas,
                x => x.Equals(
                    "Nombre Comercial:",
                    StringComparison.OrdinalIgnoreCase));

            if (indice < 0)
                return Array.Empty<string>();

            return lineas
                .Skip(indice + 1)
                .Take(5)
                .ToArray();
        }
    }
}
