using GestioneCommerciale.Components;
using QuestPDF.Infrastructure;
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Aggiunge i servizi per i componenti Razor e la modalità Interactive Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();




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
