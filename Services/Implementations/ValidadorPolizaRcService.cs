using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces.Validators;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DocumentosOCRApi.Services.Implementations
{
    public class ValidadorPolizaRcService : IValidadorPolizaRcService
    {
        public List<ResultadoValidacionDto> Validar(
            Dictionary<string, string?> datosPoliza,
            Dictionary<string, string?> datosPago)
        {
            var resultados = new List<ResultadoValidacionDto>();

            string? Obtener(
                Dictionary<string, string?> datos,
                string clave)
            {
                return datos.TryGetValue(clave, out var valor)
                    ? valor
                    : null;
            }

            // 1. Número de póliza
            var numeroPoliza = NormalizarNumero(
                Obtener(datosPoliza, "numeroPoliza"));

            var numeroPago = NormalizarNumero(
                Obtener(datosPago, "numeroPoliza"));

            resultados.Add(Comparar(
                "VAL_POLIZA_RC_001",
                numeroPoliza,
                numeroPago,
                "Los números de póliza coinciden.",
                "El número de póliza no coincide con el comprobante de pago.",
                "No fue posible comparar los números de póliza."));

            // 2. Importe
            var importePoliza = NormalizarImporte(
                Obtener(datosPoliza, "importePagar"));

            var importePago = NormalizarImporte(
                Obtener(datosPago, "importePagado"));

            resultados.Add(Comparar(
                "VAL_POLIZA_RC_002",
                importePoliza,
                importePago,
                "El importe pagado coincide con el importe de la póliza.",
                "El importe pagado es diferente al importe de la póliza.",
                "No fue posible comparar los importes."));

            // 3. Periodo de cobertura
            var inicioPoliza = NormalizarFecha(
                Obtener(datosPoliza, "fechaInicioVigencia"));

            var finPoliza = NormalizarFecha(
                Obtener(datosPoliza, "fechaFinVigencia"));

            var inicioPago = NormalizarFecha(
                Obtener(datosPago, "fechaInicioPeriodo"));

            var finPago = NormalizarFecha(
                Obtener(datosPago, "fechaFinPeriodo"));

            resultados.Add(Comparar(
                "VAL_POLIZA_RC_003",
                inicioPoliza != null && finPoliza != null
                    ? $"{inicioPoliza}|{finPoliza}"
                    : null,
                inicioPago != null && finPago != null
                    ? $"{inicioPago}|{finPago}"
                    : null,
                "El periodo del comprobante coincide con la vigencia de la póliza.",
                "El periodo del comprobante no coincide con la vigencia de la póliza.",
                "No fue posible comparar los periodos de cobertura."));

            return resultados;
        }

        private static ResultadoValidacionDto Comparar(
            string codigo,
            string? valor1,
            string? valor2,
            string mensajeValido,
            string mensajeInconsistente,
            string mensajeRevision)
        {
            if (string.IsNullOrWhiteSpace(valor1) ||
                string.IsNullOrWhiteSpace(valor2))
            {
                return new ResultadoValidacionDto
                {
                    CodigoRegla = codigo,
                    Estado = "REQUIERE_REVISION",
                    Mensaje = mensajeRevision
                };
            }

            var coincide = string.Equals(
                valor1,
                valor2,
                StringComparison.OrdinalIgnoreCase);

            return new ResultadoValidacionDto
            {
                CodigoRegla = codigo,
                Estado = coincide ? "VALIDO" : "INCONSISTENTE",
                Mensaje = coincide
                    ? mensajeValido
                    : mensajeInconsistente
            };
        }

        private static string? NormalizarNumero(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var numero = Regex.Replace(valor, @"\s+", "");

            if (!Regex.IsMatch(numero, @"^\d+$"))
                return null;

            var normalizado = numero.TrimStart('0');

            return normalizado.Length == 0 ? "0" : normalizado;
        }

        private static string? NormalizarImporte(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var limpio = valor.Replace("$", "")
                              .Replace(",", "")
                              .Trim();

            if (!decimal.TryParse(
                limpio,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var importe))
            {
                return null;
            }

            return importe.ToString(
                "F2",
                CultureInfo.InvariantCulture);
        }

        private static string? NormalizarFecha(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            if (!DateTime.TryParseExact(
                valor,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha))
            {
                return null;
            }

            return fecha.ToString("yyyy-MM-dd");
        }
    }
}
