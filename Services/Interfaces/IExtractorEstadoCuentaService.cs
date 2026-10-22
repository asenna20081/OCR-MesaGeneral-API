using DocumentosOCRApi.DTOs;

namespace DocumentosOCRApi.Services.Interfaces
{
    public interface IExtractorEstadoCuentaService
    {
        DatosEstadoCuentaDto ExtraerDatos(string textoOcr);
    }
}
