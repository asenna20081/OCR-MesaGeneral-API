using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces;
using System.Text.RegularExpressions;

namespace DocumentosOCRApi.Services
{
    public class ExtractorIneService : IExtractorIneService
    {
        public DatosIneDto ExtraerDatos(string textoOcr)
        {
            var lineas = textoOcr
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(linea => linea.Trim())
                .Where(linea => !string.IsNullOrWhiteSpace(linea))
                .ToList();

            var datos = new DatosIneDto();

            ExtraerNombre(lineas, datos);
            ExtraerSexo(textoOcr, datos);
            ExtraerCurp(textoOcr, datos);
            ExtraerFechaNacimiento(lineas, datos);
            ExtraerVigencia(lineas, datos);

            return datos;
        }

        private static void ExtraerNombre(
            List<string> lineas,
            DatosIneDto datos)
        {
            var indiceNombre = lineas.FindIndex(
                linea => linea.Equals(
                    "NOMBRE",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceNombre < 0)
                return;

            // Esperamos:
            // NOMBRE
            // APELLIDO PATERNO
            // APELLIDO MATERNO
            // NOMBRE(S)

            if (indiceNombre + 1 < lineas.Count)
                datos.ApellidoPaterno = lineas[indiceNombre + 1];

            if (indiceNombre + 2 < lineas.Count)
                datos.ApellidoMaterno = lineas[indiceNombre + 2];

            if (indiceNombre + 3 < lineas.Count)
                datos.Nombre = lineas[indiceNombre + 3];
        }

        private static void ExtraerSexo(
            string textoOcr,
            DatosIneDto datos)
        {
            var coincidencia = Regex.Match(
                textoOcr,
                @"SEXO\s+([HM])",
                RegexOptions.IgnoreCase);

            if (coincidencia.Success)
            {
                datos.Sexo = coincidencia.Groups[1].Value.ToUpper();
            }
        }

        private static void ExtraerCurp(
            string textoOcr,
            DatosIneDto datos)
        {
            var coincidencia = Regex.Match(
                textoOcr,
                @"\b[A-Z]{4}\d{6}[HM][A-Z]{5}[A-Z0-9]\d\b",
                RegexOptions.IgnoreCase);

            if (coincidencia.Success)
            {
                datos.Curp = coincidencia.Value.ToUpper();
            }
        }

        private static void ExtraerFechaNacimiento( List<string> lineas, DatosIneDto datos)
        {
            var indiceFecha = lineas.FindIndex(
                linea => linea.Contains(
                    "FECHA DE NACIMIENTO",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceFecha < 0)
                return;

            // Revisar hasta 6 líneas después de la etiqueta.
            for (int i = indiceFecha + 1;
                 i < Math.Min(indiceFecha + 7, lineas.Count);
                 i++)
            {
                var coincidencia = Regex.Match(
                    lineas[i],
                    @"\b\d{2}/\d{2}/\d{4}\b");

                if (!coincidencia.Success)
                    continue;

                if (DateTime.TryParseExact(
                    coincidencia.Value,
                    "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var fecha))
                {
                    // Evitar aceptar fechas futuras.
                    if (fecha.Date <= DateTime.Today)
                    {
                        datos.FechaNacimiento =
                            fecha.ToString("dd/MM/yyyy");

                        return;
                    }
                }
            }
        }

        private static void ExtraerVigencia( List<string> lineas, DatosIneDto datos)
        {
            var indiceVigencia = lineas.FindIndex(
                linea => linea.Contains(
                    "VIGENCIA",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceVigencia < 0)
                return;

            // Buscar primero un rango de vigencia: 2023-2033.
            for (int i = indiceVigencia;
                 i < Math.Min(indiceVigencia + 7, lineas.Count);
                 i++)
            {
                var coincidencia = Regex.Match(
                    lineas[i],
                    @"\b(20\d{2})\s*[-–]\s*(20\d{2})\b");

                if (!coincidencia.Success)
                    continue;

                int anioInicio = int.Parse(
                    coincidencia.Groups[1].Value);

                int anioFin = int.Parse(
                    coincidencia.Groups[2].Value);

                // Validación básica del rango.
                if (anioFin > anioInicio &&
                    anioFin - anioInicio <= 15)
                {
                    datos.Vigencia = anioFin;
                    return;
                }
            }

            // Si no hay rango, buscar una vigencia expresada
            // como "VIGENCIA 2033".
            var coincidenciaAnio = Regex.Match(
                lineas[indiceVigencia],
                @"\bVIGENCIA\s*:?\s*(20\d{2})\b",
                RegexOptions.IgnoreCase);

            if (coincidenciaAnio.Success)
            {
                datos.Vigencia = int.Parse(
                    coincidenciaAnio.Groups[1].Value);
            }
        }
    }
}
