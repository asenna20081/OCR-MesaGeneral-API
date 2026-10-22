using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DocumentosOCRApi.Services.Implementations
{
    public class ExtractorPolizaRcService : IExtractorPolizaRcService
    {
        public DatosPolizaRcDto ExtraerDatos(string textoOcr)
        {
            var lineas = textoOcr
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var datos = new DatosPolizaRcDto();

            ExtraerAseguradora(textoOcr, datos);
            ExtraerNumeroPoliza(textoOcr, datos);
            ExtraerContratante(lineas, datos);
            ExtraerRfc(lineas, datos);
            ExtraerTipoSeguro(textoOcr, datos);
            ExtraerVigencia(textoOcr, datos);
            ExtraerActividadAsegurada(textoOcr, datos);
            ExtraerLimiteResponsabilidad(textoOcr, datos);
            ExtraerImportePagar(lineas, datos);
            ExtraerFormaPagoYMoneda(lineas, datos);

            return datos;
        }

        private static void ExtraerAseguradora(
            string textoOcr,
            DatosPolizaRcDto datos)
        {
            if (Regex.IsMatch(
                textoOcr,
                @"GRUPO NACIONAL PROVINCIAL|\bGNP\b",
                RegexOptions.IgnoreCase))
            {
                datos.Aseguradora = "GNP SEGUROS";
            }
        }

        private static void ExtraerNumeroPoliza(
            string textoOcr,
            DatosPolizaRcDto datos)
        {
            var coincidencia = Regex.Match(
                textoOcr,
                @"No\.?\s*P[oó]liza\s*:?\s*(\d+)",
                RegexOptions.IgnoreCase);

            if (coincidencia.Success)
                datos.NumeroPoliza = coincidencia.Groups[1].Value;
        }

        private static void ExtraerContratante(
            List<string> lineas,
            DatosPolizaRcDto datos)
        {
            var indiceContratante = lineas.FindIndex(x =>
                x.Equals("CONTRATANTE",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceContratante < 0)
                return;

            var indiceNombre = lineas.FindIndex(
                indiceContratante + 1,
                x => x.Equals("Nombre",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceNombre >= 0 && indiceNombre + 1 < lineas.Count)
                datos.Contratante = lineas[indiceNombre + 1];
        }

        private static void ExtraerRfc(
            List<string> lineas,
            DatosPolizaRcDto datos)
        {
            // Buscar después de CONTRATANTE para evitar
            // tomar el RFC de la aseguradora.
            var indiceContratante = lineas.FindIndex(x =>
                x.Equals("CONTRATANTE",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceContratante < 0)
                return;

            var indiceRfc = lineas.FindIndex(
                indiceContratante + 1,
                x => Regex.IsMatch(
                    x,
                    @"^R\.?\s*F\.?\s*C\.?$",
                    RegexOptions.IgnoreCase));

            if (indiceRfc < 0)
                return;

            for (int i = indiceRfc + 1;
                 i < Math.Min(indiceRfc + 4, lineas.Count);
                 i++)
            {
                var coincidencia = Regex.Match(
                    lineas[i],
                    @"\b[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}\b",
                    RegexOptions.IgnoreCase);

                if (coincidencia.Success)
                {
                    datos.RfcContratante =
                        coincidencia.Value.ToUpperInvariant();
                    return;
                }
            }
        }

        private static void ExtraerTipoSeguro(
            string textoOcr,
            DatosPolizaRcDto datos)
        {
            var coincidencia = Regex.Match(
                textoOcr,
                @"SEGURO DE RC PROFESIONAL\s*\n\s*AGENTES DE SEGUROS Y/O DE FIANZAS",
                RegexOptions.IgnoreCase);

            if (coincidencia.Success)
            {
                datos.TipoSeguro =
                    "RC PROFESIONAL AGENTES DE SEGUROS Y/O DE FIANZAS";
            }
        }

        private static void ExtraerVigencia(
            string textoOcr,
            DatosPolizaRcDto datos)
        {
            // Buscar específicamente VIGENCIA DE LA PÓLIZA,
            // no VIGENCIA DE LA VERSIÓN.
            var coincidencia = Regex.Match(
                textoOcr,
                @"Vigencia de la P[oó]liza\s*" +
                @"Desde las\s+\d+\s+hrs\s+del\s+(\d{1,2}/[A-Za-z]{3}/\d{4})\s*" +
                @"Hasta las\s+\d+\s+hrs\s+del\s+(\d{1,2}/[A-Za-z]{3}/\d{4})",
                RegexOptions.IgnoreCase);

            if (!coincidencia.Success)
                return;

            datos.FechaInicioVigencia =
                ConvertirFecha(coincidencia.Groups[1].Value);

            datos.FechaFinVigencia =
                ConvertirFecha(coincidencia.Groups[2].Value);
        }

        private static void ExtraerActividadAsegurada(
            string textoOcr,
            DatosPolizaRcDto datos)
        {
            var coincidencia = Regex.Match(
                textoOcr,
                @"SERVICIO DE INTERMEDIARIOS DE SEGUROS Y/O DE FIANZAS",
                RegexOptions.IgnoreCase);

            if (coincidencia.Success)
                datos.ActividadAsegurada = coincidencia.Value.TrimEnd('.');
        }

        private static void ExtraerLimiteResponsabilidad(
            string textoOcr,
            DatosPolizaRcDto datos)
        {
            var coincidencia = Regex.Match(
                textoOcr,
                @"L\.U\.C\.\*?\s*\$?\s*([\d,]+\.\d{2})",
                RegexOptions.IgnoreCase);

            if (coincidencia.Success)
                datos.LimiteResponsabilidad =
                    NormalizarImporte(coincidencia.Groups[1].Value);
        }

        private static void ExtraerImportePagar(
            List<string> lineas,
            DatosPolizaRcDto datos)
        {
            var indice = lineas.FindIndex(x =>
                x.Equals("Importe por Pagar",
                    StringComparison.OrdinalIgnoreCase));

            if (indice < 0)
                return;

            for (int i = indice + 1;
                 i < Math.Min(indice + 4, lineas.Count);
                 i++)
            {
                var coincidencia = Regex.Match(
                    lineas[i],
                    @"\$?\s*([\d,]+\.\d{2})");

                if (coincidencia.Success)
                {
                    datos.ImportePagar =
                        NormalizarImporte(coincidencia.Groups[1].Value);
                    return;
                }
            }
        }

        private static void ExtraerFormaPagoYMoneda(
            List<string> lineas,
            DatosPolizaRcDto datos)
        {
            var indice = lineas.FindIndex(x =>
                x.Equals("Forma de Pago",
                    StringComparison.OrdinalIgnoreCase));

            if (indice < 0)
                return;

            // Formato observado:
            // Forma de Pago
            // Moneda
            // Anual
            // Nacional
            if (indice + 3 < lineas.Count &&
                lineas[indice + 1].Equals("Moneda",
                    StringComparison.OrdinalIgnoreCase))
            {
                datos.FormaPago = lineas[indice + 2].ToUpperInvariant();
                datos.Moneda = lineas[indice + 3].ToUpperInvariant();
            }
        }

        private static string? ConvertirFecha(string valor)
        {
            var formatos = new[] { "d/MMM/yyyy", "dd/MMM/yyyy" };

            var cultura = CultureInfo.GetCultureInfo("es-MX");

            if (DateTime.TryParseExact(
                valor,
                formatos,
                cultura,
                DateTimeStyles.None,
                out var fecha))
            {
                return fecha.ToString("dd/MM/yyyy");
            }

            return null;
        }

        private static string NormalizarImporte(string valor)
        {
            return valor.Replace(",", "").Trim();
        }
    }
}
