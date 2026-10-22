using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces.Validators;

namespace DocumentosOCRApi.Services.Implementations
{

    public class ValidadorVigenciaIneService : IValidadorVigenciaIneService
    {
        public ResultadoValidacionDto Validar(int? anioVigencia)
        {
            const string codigoRegla = "VAL_VIGENCIA_INE_001";

            if (!anioVigencia.HasValue)
            {
                return new ResultadoValidacionDto
                {
                    CodigoRegla = codigoRegla,
                    Estado = "REQUIERE_REVISION",
                    Mensaje = "No fue posible identificar la vigencia de la INE."
                };
            }

            int anioActual = DateTime.Today.Year;

            if (anioVigencia.Value < anioActual)
            {
                return new ResultadoValidacionDto
                {
                    CodigoRegla = codigoRegla,
                    Estado = "INCONSISTENTE",
                    Mensaje = $"La INE presenta un año de vigencia anterior al actual: {anioVigencia.Value}."
                };
            }

            if (anioVigencia.Value == anioActual)
            {
                return new ResultadoValidacionDto
                {
                    CodigoRegla = codigoRegla,
                    Estado = "REQUIERE_REVISION",
                    Mensaje = $"La INE indica vigencia {anioVigencia.Value}. Es necesario confirmar su fecha exacta de vencimiento."
                };
            }

            return new ResultadoValidacionDto
            {
                CodigoRegla = codigoRegla,
                Estado = "VALIDO",
                Mensaje = $"La INE indica vigencia hasta el año {anioVigencia.Value}."
            };
        }
    }

}
