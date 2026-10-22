using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces.Validators;

namespace DocumentosOCRApi.Services.Implementations
{
    public class ValidadorTipoPersonaService : IValidadorTipoPersonaService
    {
        public ResultadoValidacionDto Validar( int? idTipoPersona, Dictionary<string, string?> datosExtraidos)
        {
            var resultado = new ResultadoValidacionDto
            {
                CodigoRegla = "VAL_TIPO_PERSONA_001"
            };

            if (idTipoPersona != 1 && idTipoPersona != 2)
            {
                resultado.Estado = "REQUIERE_REVISION";
                resultado.Mensaje =
                    "El expediente no tiene un tipo de persona válido.";

                return resultado;
            }

            if (!datosExtraidos.TryGetValue(
                    "tipoPersona", out var tipoDetectado) ||
                string.IsNullOrWhiteSpace(tipoDetectado))
            {
                resultado.Estado = "REQUIERE_REVISION";
                resultado.Mensaje =
                    "No fue posible identificar el tipo de persona en la constancia fiscal.";

                return resultado;
            }

            var tipoEsperado = idTipoPersona == 1
                ? "FISICA"
                : "MORAL";

            if (string.Equals(
                tipoEsperado,
                tipoDetectado.Trim(),
                StringComparison.OrdinalIgnoreCase))
            {
                resultado.Estado = "VALIDO";
                resultado.Mensaje =
                    "El tipo de persona coincide con el expediente.";
            }
            else if (tipoDetectado.Equals(
                         "FISICA", StringComparison.OrdinalIgnoreCase) ||
                     tipoDetectado.Equals(
                         "MORAL", StringComparison.OrdinalIgnoreCase))
            {
                resultado.Estado = "INCONSISTENTE";
                resultado.Mensaje =
                    $"El expediente corresponde a persona {tipoEsperado}, " +
                    $"pero la constancia fiscal corresponde a persona {tipoDetectado}.";
            }
            else
            {
                resultado.Estado = "REQUIERE_REVISION";
                resultado.Mensaje =
                    "El tipo de persona extraído no es reconocido.";
            }

            return resultado;
        }
    }
}
