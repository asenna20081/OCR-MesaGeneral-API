using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DocumentosOCRApi.Services.Implementations
{
    public class ExtractorEstadoCuentaService : IExtractorEstadoCuentaService
    {
        public DatosEstadoCuentaDto ExtraerDatos(string textoOcr)
        {
            var lineas = textoOcr
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var datos = new DatosEstadoCuentaDto();

            ExtraerBanco(textoOcr, datos);
            ExtraerTitular(lineas, datos);
            ExtraerRfc(lineas, datos);
            ExtraerCuentaYClabe(lineas, datos);
            ExtraerFechas(lineas, datos);
            ExtraerMoneda(lineas, datos);

            return datos;
        }

        private static void ExtraerBanco(
            string textoOcr,
            DatosEstadoCuentaDto datos)
        {
            if (textoOcr.Contains(
                "SCOTIABANK",
                StringComparison.OrdinalIgnoreCase))
            {
                datos.Banco = "SCOTIABANK";
            }
        }

        private static void ExtraerTitular(
            List<string> lineas,
            DatosEstadoCuentaDto datos)
        {
            // En este formato, el titular aparece después
            // de la línea PAGINA 1 DE 4.
            var indicePagina = lineas.FindIndex(x =>
                Regex.IsMatch(
                    x,
                    @"P[AÁ]GINA\s+\d+\s+DE\s+\d+",
                    RegexOptions.IgnoreCase));

            if (indicePagina >= 0 &&
                indicePagina + 1 < lineas.Count)
            {
                datos.Titular = lineas[indicePagina + 1];
            }
        }

        private static void ExtraerRfc(
            List<string> lineas,
            DatosEstadoCuentaDto datos)
        {
            var indiceRfc = lineas.FindIndex(x =>
                Regex.IsMatch(
                    x,
                    @"R\.?\s*F\.?\s*C\.?\s*CLIENTE",
                    RegexOptions.IgnoreCase));

            if (indiceRfc < 0)
                return;

            // El valor puede aparecer varias líneas después
            // porque Document AI lee primero las etiquetas.
            for (int i = indiceRfc + 1;
                 i < Math.Min(indiceRfc + 5, lineas.Count);
                 i++)
            {
                var coincidencia = Regex.Match(
                    lineas[i],
                    @"\b[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}\b",
                    RegexOptions.IgnoreCase);

                if (coincidencia.Success)
                {
                    datos.Rfc =
                        coincidencia.Value.ToUpperInvariant();
                    return;
                }
            }
        }

        private static void ExtraerCuentaYClabe(
            List<string> lineas,
            DatosEstadoCuentaDto datos)
        {
            // En el documento se observa:
            // Cuenta
            // CLABE
            // Fecha de corte
            // Periodo
            // ...
            // 11006207790
            // 044770110062077901

            var indiceCuenta = lineas.FindIndex(x =>
                x.Equals(
                    "Cuenta",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceCuenta < 0)
                return;

            // Buscar después del encabezado, antes del resumen
            // de movimientos y saldos.
            var limite = Math.Min(indiceCuenta + 18, lineas.Count);

            for (int i = indiceCuenta + 1; i < limite; i++)
            {
                if (datos.NumeroCuenta == null &&
                    Regex.IsMatch(lineas[i], @"^\d{10,12}$"))
                {
                    datos.NumeroCuenta = lineas[i];
                }

                if (datos.Clabe == null &&
                    Regex.IsMatch(lineas[i], @"^\d{18}$"))
                {
                    datos.Clabe = lineas[i];
                }

                if (datos.NumeroCuenta != null &&
                    datos.Clabe != null)
                {
                    break;
                }
            }
        }

        private static void ExtraerFechas(
            List<string> lineas,
            DatosEstadoCuentaDto datos)
        {
            var indiceFechaCorte = lineas.FindIndex(x =>
                x.Equals(
                    "Fecha de corte",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceFechaCorte < 0)
                return;

            var limite = Math.Min(indiceFechaCorte + 18, lineas.Count);

            for (int i = indiceFechaCorte + 1; i < limite; i++)
            {
                // Ejemplo: 02-DIC-25/29-DIC-25
                var periodo = Regex.Match(
                    lineas[i],
                    @"(\d{2}-[A-Z]{3}-\d{2})\s*/\s*(\d{2}-[A-Z]{3}-\d{2})",
                    RegexOptions.IgnoreCase);

                if (periodo.Success)
                {
                    datos.FechaInicioPeriodo =
                        ConvertirFecha(periodo.Groups[1].Value);

                    datos.FechaFinPeriodo =
                        ConvertirFecha(periodo.Groups[2].Value);

                    continue;
                }

                // Ejemplo: 29-DIC-25
                if (datos.FechaCorte == null &&
                    Regex.IsMatch(
                        lineas[i],
                        @"^\d{2}-[A-Z]{3}-\d{2}$",
                        RegexOptions.IgnoreCase))
                {
                    datos.FechaCorte =
                        ConvertirFecha(lineas[i]);
                }
            }
        }

        private static void ExtraerMoneda(
            List<string> lineas,
            DatosEstadoCuentaDto datos)
        {
            // En este formato el valor aparece en el bloque
            // de datos generales de la cuenta.
            var indiceMoneda = lineas.FindIndex(x =>
                x.Equals(
                    "Moneda",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceMoneda < 0)
                return;

            for (int i = indiceMoneda + 1;
                 i < Math.Min(indiceMoneda + 12, lineas.Count);
                 i++)
            {
                if (lineas[i].Equals(
                    "NACIONAL",
                    StringComparison.OrdinalIgnoreCase))
                {
                    datos.Moneda = "NACIONAL";
                    return;
                }
            }
        }

        private static string? ConvertirFecha(string valor)
        {
            var meses = new Dictionary<string, string>
            {
                ["ENE"] = "01",
                ["FEB"] = "02",
                ["MAR"] = "03",
                ["ABR"] = "04",
                ["MAY"] = "05",
                ["JUN"] = "06",
                ["JUL"] = "07",
                ["AGO"] = "08",
                ["SEP"] = "09",
                ["OCT"] = "10",
                ["NOV"] = "11",
                ["DIC"] = "12"
            };

            var partes = valor.ToUpperInvariant().Split('-');

            if (partes.Length != 3 ||
                !meses.TryGetValue(partes[1], out var mes))
            {
                return null;
            }

            var fechaTexto =
                $"{partes[0]}/{mes}/20{partes[2]}";

            if (DateTime.TryParseExact(
                fechaTexto,
                "dd/MM/yyyy",
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
