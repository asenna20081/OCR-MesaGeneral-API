using DocumentosOCRApi.Data;
using DocumentosOCRApi.Services;
using DocumentosOCRApi.Services.Implementations;
using DocumentosOCRApi.Services.Interfaces;
using DocumentosOCRApi.Services.Interfaces.Validators;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDocumentoService, DocumentoService>();
builder.Services.AddScoped<IOcrService, GoogleDocumentAiOcrService>();
builder.Services.AddScoped<IExtractorIneService, ExtractorIneService>();
builder.Services.AddScoped<IProcesoService, ProcesoService>();
builder.Services.AddScoped<IExtractorActaService, ExtractorActaService>();
builder.Services.AddScoped<IExtractorConstanciaFiscalService, ExtractorConstanciaFiscalService>();
builder.Services.AddScoped<IValidadorTipoPersonaService, ValidadorTipoPersonaService>();
builder.Services.AddScoped<IExtractorCurpService, ExtractorCurpService>();
builder.Services.AddScoped<IValidadorCurpService, ValidadorCurpService>();
builder.Services.AddScoped<IValidadorIdentidadIneService, ValidadorIdentidadIneService>();
builder.Services.AddScoped<IExtractorComprobanteDomicilioService, ExtractorComprobanteDomicilioService>();
builder.Services.AddScoped<IValidadorFormatoDocumentoService, ValidadorFormatoDocumentoService>();
builder.Services.AddScoped<IValidadorVigenciaIneService, ValidadorVigenciaIneService>();
builder.Services.AddScoped<IExtractorCedulaAgenteService, ExtractorCedulaAgenteService>();
builder.Services.AddScoped<IExtractorEstadoCuentaService, ExtractorEstadoCuentaService>();
builder.Services.AddScoped<IExtractorPolizaRcService, ExtractorPolizaRcService>();
builder.Services.AddScoped<IExtractorPagoPolizaRcService, ExtractorPagoPolizaRcService>();
builder.Services.AddScoped<IValidadorPolizaRcService, ValidadorPolizaRcService>();
builder.Services.AddDbContext<DocumentosOcrDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DocumentosOCR")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();
