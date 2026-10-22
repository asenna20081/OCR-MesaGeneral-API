using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces
{
    public interface IExtractorIneService
    {
        DatosIneDto ExtraerDatos(string textoOcr);
    }
}
