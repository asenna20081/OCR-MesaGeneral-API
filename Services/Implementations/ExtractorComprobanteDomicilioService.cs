using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces;
using System.Text.RegularExpressions;

namespace DocumentosOCRApi.Services.Implementations
{
    public class ExtractorComprobanteDomicilioService : IExtractorComprobanteDomicilioService
    {
        public DatosComprobanteDomicilioDto ExtraerDatos(string textoOcr)
        {
            var datos = new DatosComprobanteDomicilioDto();

            if (string.IsNullOrWhiteSpace(textoOcr))
                return datos;

            var lineas = textoOcr
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            // Solo analizamos el bloque anterior al número de servicio
            // para evitar tomar la dirección de las oficinas de CFE.
            var indiceServicio = lineas.FindIndex(x =>
                x.StartsWith(
                    "NO. DE SERVICIO",
                    StringComparison.OrdinalIgnoreCase));

            if (indiceServicio < 0)
                return datos;

            var bloqueDomicilio = lineas
                .Take(indiceServicio)
                .ToList();

            // Buscar el inicio del bloque del titular.
            var indiceEncabezado = bloqueDomicilio.FindLastIndex(x =>
                x.Contains(
                    "Comisión Federal de Electricidad",
                    StringComparison.OrdinalIgnoreCase));

            var inicio = indiceEncabezado >= 0
                ? indiceEncabezado + 1
                : -1;

            // Esta primera versión requiere reconocer el encabezado.
            if (inicio < 0 || inicio + 4 >= bloqueDomicilio.Count)
                return datos;

            datos.Titular = bloqueDomicilio[inicio];

            // Ejemplo: CDA GENOVA 89
            var calleCompleta = bloqueDomicilio[inicio + 1];

            var coincidenciaCalle = Regex.Match(
                calleCompleta,
                @"^(.*?)\s+(\d+[A-Z]?)$",
                RegexOptions.IgnoreCase);

            if (coincidenciaCalle.Success)
            {
                datos.Calle = coincidenciaCalle.Groups[1].Value.Trim();
                datos.NumeroExterior = coincidenciaCalle.Groups[2].Value;
            }
            else
            {
                datos.Calle = calleCompleta;
            }

            // Ejemplo: COSECHAS Y EJE ROSALES
            datos.ReferenciaDomicilio = bloqueDomicilio[inicio + 2];

            // Ejemplo: GENOVA RESIDENCIALC.P.83288
            var lineaColonia = bloqueDomicilio[inicio + 3];

            var coincidenciaCp = Regex.Match(
                lineaColonia,
                @"C\.?\s*P\.?\s*:?\s*(\d{5})",
                RegexOptions.IgnoreCase);

            if (coincidenciaCp.Success)
            {
                datos.CodigoPostal = coincidenciaCp.Groups[1].Value;

                datos.Colonia = lineaColonia
                    .Substring(0, coincidenciaCp.Index)
                    .Trim();
            }
            else
            {
                datos.Colonia = lineaColonia;
            }

            // Ejemplo: HERMOSILLO, Son.
            var ubicacion = bloqueDomicilio[inicio + 4];

            var partesUbicacion = ubicacion.Split(
                ',',
                StringSplitOptions.TrimEntries);

            if (partesUbicacion.Length >= 1)
                datos.Municipio = partesUbicacion[0];

            if (partesUbicacion.Length >= 2)
            {
                datos.Estado = partesUbicacion[1]
                    .TrimEnd('.')
                    .ToUpperInvariant() switch
                {
                    "SON" => "SONORA",
                    _ => partesUbicacion[1].Trim()
                };
            }

            // Número de servicio
            var coincidenciaServicio = Regex.Match(
                textoOcr,
                @"NO\.\s*DE\s*SERVICIO\s*:\s*(\d+)",
                RegexOptions.IgnoreCase);

            if (coincidenciaServicio.Success)
            {
                datos.NumeroServicio =
                    coincidenciaServicio.Groups[1].Value;
            }

            // Fecha límite de pago
            var coincidenciaFecha = Regex.Match(
                textoOcr,
                @"L[IÍ]MITE\s+DE\s+PAGO\s*:\s*(\d{1,2}\s+[A-Z]{3}\s+\d{2,4})",
                RegexOptions.IgnoreCase);

            if (coincidenciaFecha.Success)
            {
                datos.FechaLimitePago =
                    coincidenciaFecha.Groups[1].Value.Trim();
            }

            return datos;
        }
    }
}
