using System.Text.RegularExpressions;
using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces;
using Google.Cloud.DocumentAI.V1;

namespace DocumentosOCRApi.Services.Implementations
{
    public class ExtractorCurpService : IExtractorCurpService
    {
        public ResultadoExtraccionDto ExtraerDatos(
            Document documento,
            int idTipoDocumento)
        {
            var texto = documento.Text ?? string.Empty;

            var resultado = new ResultadoExtraccionDto
            {
                IdTipoDocumento = idTipoDocumento,
                ClaveDocumento = 1443,
                TipoDocumento = "CURP CONSTANCIA"
            };

            // 1. Extraer CURP.
            var coincidenciaCurp = Regex.Match(
                texto,
                @"\b[A-Z]{4}\d{6}[HM][A-Z]{5}[A-Z0-9]\d\b",
                RegexOptions.IgnoreCase);

            if (coincidenciaCurp.Success)
            {
                resultado.Datos.Add(new DatosExtraidosDto
                {
                    Campo = "curp",
                    Valor = coincidenciaCurp.Value.ToUpperInvariant()
                });
            }

            // 2. Extraer nombre completo.
            var nombreCompleto = ExtraerNombreCompleto(texto);

            if (!string.IsNullOrWhiteSpace(nombreCompleto))
            {
                resultado.Datos.Add(new DatosExtraidosDto
                {
                    Campo = "nombreCompleto",
                    Valor = nombreCompleto
                });
            }

            // 3. Extraer entidad de registro.
            var entidad = Regex.Match(
                texto,
                @"Entidad de registro\s*:\s*([^\r\n]+)",
                RegexOptions.IgnoreCase);

            if (entidad.Success)
            {
                resultado.Datos.Add(new DatosExtraidosDto
                {
                    Campo = "entidadRegistro",
                    Valor = entidad.Groups[1].Value.Trim()
                });
            }

            // 4. Identificar certificación.
            if (texto.Contains(
                "CURP Certificada: verificada con el Registro Civil",
                StringComparison.OrdinalIgnoreCase))
            {
                resultado.Datos.Add(new DatosExtraidosDto
                {
                    Campo = "certificada",
                    Valor = "SI"
                });
            }

            return resultado;
        }

        private static string ExtraerNombreCompleto(string texto)
        {
            var lineas = texto
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            int indiceNombre = lineas.FindIndex(x =>
                x.Equals("Nombre", StringComparison.OrdinalIgnoreCase) ||
                x.Equals("Nombre:", StringComparison.OrdinalIgnoreCase));

            if (indiceNombre == -1)
                return string.Empty;

            // Buscar en las siguientes líneas.
            var candidatos = lineas
                .Skip(indiceNombre + 1)
                .Take(5)
                .Where(x =>
                    Regex.IsMatch(x, @"^[A-ZÁÉÍÓÚÜÑ\s]+$") &&
                    x.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 3)
                .ToList();

            return candidatos.FirstOrDefault() ?? string.Empty;
        }
    }
}