using GestioneCommerciale.Components;
using GestioneCommerciale.Services;
using QuestPDF.Infrastructure;
using Radzen;






var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;


// Aggiunge i servizi per i componenti Razor e la modalità Interactive Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        // Alza il limite per permettere l'invio di file PDF grandi (10MB)
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
    });
builder.Services.AddHttpClient();
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<PianificatoreService>();
builder.Services.AddScoped<VisitaService>();


var app = builder.Build();

// Configurazione della pipeline delle richieste HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // Il valore HSTS predefinito è 30 giorni.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

// Gestione degli asset statici (CSS, JS, immagini)
app.MapStaticAssets();

// Mapping dei componenti Blazor e attivazione della modalità Interactive Server
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
