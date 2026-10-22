using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces.Validators
{
    public class ValidadorIdentidadIneService : IValidadorIdentidadIneService
    {
        public ResultadoValidacionDto Validar(  string? curpIne, string? curpConstancia)
        {
            const string codigoRegla = "VAL_IDENTIDAD_003";

            if (string.IsNullOrWhiteSpace(curpIne) ||
                string.IsNullOrWhiteSpace(curpConstancia))
            {
                return new ResultadoValidacionDto
                {
                    CodigoRegla = codigoRegla,
                    Estado = "REQUIERE_REVISION",
                    Mensaje = "No fue posible comparar la CURP de la INE con la Constancia CURP."
                };
            }

            var curpIneNormalizada = curpIne.Trim().ToUpperInvariant();
            var curpConstanciaNormalizada = curpConstancia.Trim().ToUpperInvariant();

            if (curpIneNormalizada == curpConstanciaNormalizada)
            {
                return new ResultadoValidacionDto
                {
                    CodigoRegla = codigoRegla,
                    Estado = "VALIDO",
                    Mensaje = "La CURP de la INE coincide con la Constancia CURP."
                };
            }

            return new ResultadoValidacionDto
            {
                CodigoRegla = codigoRegla,
                Estado = "INCONSISTENTE",
                Mensaje = "La CURP de la INE no coincide con la Constancia CURP."
            };
        }
    }
}
