using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces.Validators;

namespace DocumentosOCRApi.Services.Implementations
{
    public class ValidadorCurpService : IValidadorCurpService
    {
        public ResultadoValidacionDto Validar(
            string? curpFiscal,
            string? curpRenapo)
        {
            var resultado = new ResultadoValidacionDto
            {
                CodigoRegla = "VAL_IDENTIDAD_002"
            };

            if (string.IsNullOrWhiteSpace(curpFiscal) ||
                string.IsNullOrWhiteSpace(curpRenapo))
            {
                resultado.Estado = "REQUIERE_REVISION";
                resultado.Mensaje =
                    "No fue posible comparar la CURP porque falta información en alguno de los documentos.";

                return resultado;
            }

            var fiscal = curpFiscal.Trim().ToUpperInvariant();
            var renapo = curpRenapo.Trim().ToUpperInvariant();

            if (fiscal == renapo)
            {
                resultado.Estado = "VALIDO";
                resultado.Mensaje =
                    "La CURP de la constancia fiscal coincide con la constancia RENAPO.";
            }
            else
            {
                resultado.Estado = "INCONSISTENTE";
                resultado.Mensaje =
                    "La CURP de la constancia fiscal no coincide con la constancia RENAPO.";
            }

            return resultado;
        }
    }
}
