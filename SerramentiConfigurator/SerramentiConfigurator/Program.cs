using Microsoft.EntityFrameworkCore;
using SerramentiConfigurator.Data;
using SerramentiConfigurator.Components;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Aggiunge i servizi per la gestione dei componenti interattivi Blazor e Controller API
builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
builder.Services.AddControllers();

// REGISTRAZIONE MOTORE SQL SERVER EXPRESS (Connessione locale all'ufficio)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =========================================================================
// 🚀 CONFIGURAZIONE OPENAPI NATIVA .NET 10 & SCALAR (Anti TypeLoadException)
// =========================================================================
// Sostituisce AddEndpointsApiExplorer() e AddSwaggerGen()
builder.Services.AddOpenApi();

// Registra il servizio del motore di calcolo per l'elaborazione dei preventivi al KG
builder.Services.AddScoped<SerramentiConfigurator.Server.Services.MotoreCalcoloCommerciale>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();

    // ABILITAZIONE DELL'INFRASTRUTTURA OPENAPI NATIVA IN MODALITÀ SVILUPPO
    // Genera l'endpoint JSON della specifica su: /openapi/v1.json
    app.MapOpenApi();

    // Attiva l'interfaccia grafica di test moderna su: /scalar/v1
    // Sostituisce app.UseSwagger() e app.UseSwaggerUI()
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

// Mappatura delle risorse statiche e della pipeline Blazor WebAssembly
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SerramentiConfigurator.Client._Imports).Assembly);

app.MapControllers();

app.Run();
