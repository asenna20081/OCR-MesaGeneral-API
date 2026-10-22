using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DocumentosOCRApi.Services.Implementations
{
    public class ExtractorCedulaAgenteService
    : IExtractorCedulaAgenteService
    {
        public DatosCedulaAgenteDto ExtraerDatos(string textoOcr)
        {
            var lineas = textoOcr
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var datos = new DatosCedulaAgenteDto();

            ExtraerNumeroCedula(lineas, datos);
            ExtraerVigencia(textoOcr, datos);
            ExtraerRfc(textoOcr, datos);
            ExtraerCurp(textoOcr, datos);
            ExtraerNombre(lineas, datos);
            ExtraerAutorizacion(lineas, datos);

            return datos;
        }

        private static void ExtraerNumeroCedula(
            List<string> lineas,
            DatosCedulaAgenteDto datos)
        {
            // En este formato la cédula aparece cerca
            // del encabezado CNSF.
            var indiceCnsf = lineas.FindIndex(x =>
                x.Equals("CNSF", StringComparison.OrdinalIgnoreCase));

            if (indiceCnsf < 0)
                return;

            for (int i = indiceCnsf + 1;
                 i < Math.Min(indiceCnsf + 4, lineas.Count);
                 i++)
            {
                var coincidencia = Regex.Match(
                    lineas[i],
                    @"\b[A-Z]\d{6}\b",
                    RegexOptions.IgnoreCase);

                if (coincidencia.Success)
                {
                    datos.NumeroCedula =
                        coincidencia.Value.ToUpperInvariant();
                    return;
                }
            }
        }

        private static void ExtraerVigencia(
            string textoOcr,
            DatosCedulaAgenteDto datos)
        {
            var coincidencia = Regex.Match(
                textoOcr,
                @"VIGENCIA\s*:?\s*(\d{2}/\d{2}/\d{4})\s+A\s+(\d{2}/\d{2}/\d{4})",
                RegexOptions.IgnoreCase);

            if (!coincidencia.Success)
                return;

            if (DateTime.TryParseExact(
                    coincidencia.Groups[1].Value,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var inicio) &&
                DateTime.TryParseExact(
                    coincidencia.Groups[2].Value,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var fin) &&
                fin >= inicio)
            {
                datos.FechaInicioVigencia =
                    inicio.ToString("dd/MM/yyyy");

                datos.FechaFinVigencia =
                    fin.ToString("dd/MM/yyyy");
            }
        }

        private static void ExtraerRfc(
            string textoOcr,
            DatosCedulaAgenteDto datos)
        {
            var coincidencia = Regex.Match(
                textoOcr,
                @"\bRFC\s*:?\s*([A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3})\b",
                RegexOptions.IgnoreCase);

            if (coincidencia.Success)
            {
                datos.Rfc =
                    coincidencia.Groups[1].Value.ToUpperInvariant();
            }
        }

        private static void ExtraerCurp(
            string textoOcr,
            DatosCedulaAgenteDto datos)
        {
            var coincidencia = Regex.Match(
                textoOcr,
                @"\bCURP\s*:?\s*([A-Z]{4}\d{6}[HM][A-Z]{5}[A-Z0-9]\d)\b",
                RegexOptions.IgnoreCase);

            if (coincidencia.Success)
            {
                datos.Curp =
                    coincidencia.Groups[1].Value.ToUpperInvariant();
            }
        }

        private static void ExtraerNombre(
            List<string> lineas,
            DatosCedulaAgenteDto datos)
        {
            var indice = lineas.FindIndex(x =>
                Regex.IsMatch(
                    x,
                    @"\bAUTORIZA\s+A\b",
                    RegexOptions.IgnoreCase));

            if (indice < 0)
                return;

            // Buscar el nombre en la línea siguiente.
            if (indice + 1 < lineas.Count)
            {
                var nombre = lineas[indice + 1];

                if (Regex.IsMatch(
                    nombre,
                    @"^[A-ZÁÉÍÓÚÜÑ\s]+$",
                    RegexOptions.IgnoreCase) &&
                    nombre.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries).Length >= 2)
                {
                    datos.NombreCompleto = nombre;
                }
            }
        }

        private static void ExtraerAutorizacion(
            List<string> lineas,
            DatosCedulaAgenteDto datos)
        {
            var indice = lineas.FindIndex(x =>
                x.Contains(
                    "AGENTE DE SEGUROS POR CUENTA PROPIA",
                    StringComparison.OrdinalIgnoreCase));

            if (indice < 0)
                return;

            datos.TipoAutorizacion = lineas[indice];

            if (indice + 1 < lineas.Count)
            {
                var categoria = lineas[indice + 1];

                if (Regex.IsMatch(
                    categoria,
                    @"^[A-ZÁÉÍÓÚÜÑ\s]+$",
                    RegexOptions.IgnoreCase))
                {
                    datos.Categoria = categoria;
                }
            }
        }
    }
}
