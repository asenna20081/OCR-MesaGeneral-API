using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DocumentosOCRApi.Services.Implementations
{
    public class ExtractorPagoPolizaRcService
    : IExtractorPagoPolizaRcService
    {
        public DatosPagoPolizaRcDto ExtraerDatos(string textoOcr)
        {
            var lineas = textoOcr
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x) &&
                            x != "•")
                .ToList();

            var datos = new DatosPagoPolizaRcDto();

            datos.Aseguradora = Regex.IsMatch(
                textoOcr,
                @"\bGNP\b",
                RegexOptions.IgnoreCase)
                ? "GNP SEGUROS"
                : null;

            datos.NumeroPoliza = ExtraerValor(
                textoOcr,
                @"(?:No\.\s*de\s*)?P[oó]liza\s*:\s*(\d{8,20})");

            datos.Ramo = ExtraerValor(
                textoOcr,
                @"Ramo\s*:\s*([^\r\n]+)");

            datos.ImportePagado = ExtraerImportePago(textoOcr);

            datos.FechaPago = ConvertirFecha(
                ExtraerValor(
                    textoOcr,
                    @"Fecha de pago\s*:\s*(\d{2}-\d{2}-\d{4})"));

            datos.NumeroRecibo = ExtraerValor(
                textoOcr,
                @"No\.\s*de recibo\s*:\s*(\d+)");

            datos.EstatusRecibo = ExtraerValor(
                textoOcr,
                @"Estatus del recibo\s*:\s*([^\r\n]+)");

            datos.NumeroCobro = ExtraerValor(
                textoOcr,
                @"No\.\s*de cobro\s*:\s*(\d+)");

            datos.EstatusPoliza = ExtraerValor(
                textoOcr,
                @"Estatus de la P[oó]liza\s*:\s*([^\r\n]+)");

            datos.FormaPago = ExtraerValor(
                textoOcr,
                @"Forma de pago\s*:\s*([^\r\n]+)");

            datos.ConductoCobro = ExtraerValor(
                textoOcr,
                @"Conducto de cobro\s*:\s*([^\r\n]+)");

            datos.FechaInicioPeriodo = ConvertirFecha(
                ExtraerValor(
                    textoOcr,
                    @"Periodo desde\s*:\s*(\d{2}-\d{2}-\d{4})"));

            datos.FechaFinPeriodo = ConvertirFecha(
                ExtraerValor(
                    textoOcr,
                    @"Periodo hasta\s*:\s*(\d{2}-\d{2}-\d{4})"));

            ExtraerNombres(lineas, datos);

            return datos;
        }

        private static string? ExtraerValor(
            string texto,
            string patron)
        {
            var coincidencia = Regex.Match(
                texto,
                patron,
                RegexOptions.IgnoreCase);

            return coincidencia.Success
                ? coincidencia.Groups[1].Value.Trim()
                : null;
        }

        private static string? ExtraerImportePago(string texto)
        {
            // Priorizar el importe dentro de DETALLE DE PAGO
            // para no confundirlo con otros importes.
            var coincidencia = Regex.Match(
                texto,
                @"Detalle de pago\s*:\s*" +
                @"(?:[•\s]*)Importe\s*:\s*" +
                @"\$?\s*([\d,]+\.\d{2})",
                RegexOptions.IgnoreCase);

            if (!coincidencia.Success)
                return null;

            return coincidencia.Groups[1].Value.Replace(",", "");
        }

        private static void ExtraerNombres(
            List<string> lineas,
            DatosPagoPolizaRcDto datos)
        {
            // En este OCR los nombres aparecen después de
            // las etiquetas, pero separados por otros campos.
            //
            // Usamos el bloque anterior a "Estatus de la Póliza"
            // y verificamos que las dos etiquetas existan.

            var indiceContratante = lineas.FindIndex(x =>
                x.Contains(
                    "Nombre del contratante:",
                    StringComparison.OrdinalIgnoreCase));

            var indiceAsegurado = lineas.FindIndex(x =>
                x.Contains(
                    "Nombre del asegurado:",
                    StringComparison.OrdinalIgnoreCase));

            var indiceEstatus = lineas.FindIndex(x =>
                x.Contains(
                    "Estatus de la Póliza:",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceContratante < 0 ||
                indiceAsegurado < 0 ||
                indiceEstatus < 0)
                return;

            // Los nombres aparecen después de "Versión: 1"
            // y antes de "Estatus de la Póliza".
            var indiceVersion = lineas.FindIndex(
                indiceAsegurado + 1,
                x => x.StartsWith(
                    "Versión:",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceVersion < 0 ||
                indiceVersion >= indiceEstatus)
                return;

            var posiblesNombres = lineas
                .Skip(indiceVersion + 1)
                .Take(indiceEstatus - indiceVersion - 1)
                .Where(x => Regex.IsMatch(
                    x,
                    @"^[A-ZÁÉÍÓÚÜÑa-záéíóúüñ\s]+$") &&
                    x.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries).Length >= 2)
                .ToList();

            if (posiblesNombres.Count >= 2)
            {
                datos.Contratante = posiblesNombres[0];
                datos.Asegurado = posiblesNombres[1];
            }
        }

        private static string? ConvertirFecha(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            if (DateTime.TryParseExact(
                valor,
                "dd-MM-yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha))
            {
                return fecha.ToString("dd/MM/yyyy");
            }

            return null;
        }
    }
}
