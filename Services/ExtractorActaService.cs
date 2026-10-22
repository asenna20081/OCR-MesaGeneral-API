using DocumentosOCRApi.DTOs;
using DocumentosOCRApi.Services.Interfaces;
using Google.Cloud.DocumentAI.V1;
using System.Text.RegularExpressions;

namespace DocumentosOCRApi.Services
{
    public class ExtractorActaService : IExtractorActaService
    {
        private class ElementoOcr
        {
            public string Texto { get; set; } = string.Empty;
            public float X { get; set; }
            public float Y { get; set; }
            public float YMin { get; set; }
            public float YMax { get; set; }
        }
        public DatosIneDto ExtraerDatos(Document documento)
        {
            var elementos = ObtenerElementos(documento);

            //temporal
            foreach (var elemento in elementos)
            {
                if (
                    elemento.Texto.Contains("GARCIA", StringComparison.OrdinalIgnoreCase) ||
                    elemento.Texto.Contains("GARDUÑO", StringComparison.OrdinalIgnoreCase) ||
                    elemento.Texto.Contains("Primer Apellido", StringComparison.OrdinalIgnoreCase) ||
                    elemento.Texto.Contains("Segundo Apellido", StringComparison.OrdinalIgnoreCase)
                )
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Texto: {elemento.Texto} | " +
                        $"X: {elemento.X:F4} | " +
                        $"Y: {elemento.Y:F4} | " +
                        $"YMin: {elemento.YMin:F4} | " +
                        $"YMax: {elemento.YMax:F4}"
                    );
                }
            }


            var persona = ObtenerSeccionPersona(elementos);

            var nombre = BuscarValorEncima(
                persona, "Nombre(s):");

            var apellidoPaterno = BuscarValorEncima(
                persona, "Primer Apellido:");

            var apellidoMaterno = BuscarValorEncima(
                persona, "Segundo Apellido:");

            var sexo = BuscarValorEncima(
                persona, "Sexo:");

            var fechaNacimiento = BuscarValorEncima(
                persona, "Fecha de Nacimiento:");

            var curpMatch = Regex.Match(
                documento.Text,
                @"\b[A-Z]{4}\d{6}[HM][A-Z]{5}[A-Z0-9]\d\b",
                RegexOptions.IgnoreCase);

            return new DatosIneDto
            {
                Nombre = nombre,
                ApellidoPaterno = apellidoPaterno,
                ApellidoMaterno = apellidoMaterno,
                Sexo = sexo,
                FechaNacimiento = fechaNacimiento,
                Curp = curpMatch.Success
                    ? curpMatch.Value.ToUpperInvariant()
                    : string.Empty
            };
        }

        private List<ElementoOcr> ObtenerElementos(Document documento)
        {
            var elementos = new List<ElementoOcr>();

            foreach (var pagina in documento.Pages)
            {
                foreach (var linea in pagina.Lines)
                {
                    var layout = linea.Layout;

                    var texto = string.Concat(
                        layout.TextAnchor.TextSegments.Select(segmento =>
                        {
                            int inicio = (int)segmento.StartIndex;
                            int longitud = (int)(
                                segmento.EndIndex - segmento.StartIndex);

                            return documento.Text.Substring(inicio, longitud);
                        })
                    ).Trim();

                    var vertices = layout.BoundingPoly.NormalizedVertices;

                    if (vertices.Count == 0 || string.IsNullOrWhiteSpace(texto))
                        continue;

                    elementos.Add(new ElementoOcr
                    {
                        Texto = texto,
                        X = vertices.Average(v => v.X),
                        Y = vertices.Average(v => v.Y),
                        YMin = vertices.Min(v => v.Y),
                        YMax = vertices.Max(v => v.Y)
                    });
                }
            }

            return elementos;
        }

        private string BuscarValorEncima(
            List<ElementoOcr> elementos,
            string etiqueta,
            Func<string, bool>? validarValor = null,
            float distanciaMaximaY = 0.065f,
            float distanciaMaximaX = 0.18f)
        {
            var etiquetas = elementos
                .Where(x => x.Texto.Contains(
                    etiqueta,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var elementoEtiqueta in etiquetas)
            {
                var candidatos = elementos
                    .Where(x =>
                        x.Y < elementoEtiqueta.Y &&
                        elementoEtiqueta.Y - x.Y <= distanciaMaximaY &&
                        Math.Abs(x.X - elementoEtiqueta.X) <= distanciaMaximaX &&
                        (validarValor == null ||
                         validarValor(x.Texto)))
                    .OrderBy(x => elementoEtiqueta.Y - x.Y)
                    .ThenBy(x => Math.Abs(
                        x.X - elementoEtiqueta.X))
                    .ToList();

                if (candidatos.Count > 0)
                {
                    return candidatos[0].Texto;
                }
            }

            return string.Empty;
        }

        private List<ElementoOcr> ObtenerSeccionPersona(
    List<ElementoOcr> elementos)
        {
            var inicio = elementos.FirstOrDefault(x =>
                x.Texto.Contains(
                    "Datos de la Persona Registrada",
                    StringComparison.OrdinalIgnoreCase));

            var fin = elementos.FirstOrDefault(x =>
                x.Texto.Contains(
                    "Datos de Filiación",
                    StringComparison.OrdinalIgnoreCase));

            if (inicio == null || fin == null ||
                inicio.YMax >= fin.YMin)
            {
                return new List<ElementoOcr>();
            }

            return elementos
                .Where(x =>
                    x.YMin > inicio.YMax &&
                    x.YMax < fin.YMin)
                .ToList();
        }
    }
}
