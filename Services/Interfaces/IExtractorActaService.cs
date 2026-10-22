using DocumentosOCRApi.DTOs;
using Google.Cloud.DocumentAI.V1;

namespace DocumentosOCRApi.Services.Interfaces
{
    public interface IExtractorActaService
    {
        //DatosIneDto ExtraerDatos(string textoOcr);
        DatosIneDto ExtraerDatos(Document documento);
    }
}
