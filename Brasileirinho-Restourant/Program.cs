using BrasileirinhoRestourant.Application.Services;
using BrasileirinhoRestourant.Components;
using BrasileirinhoRestourant.Infrastructure.Data;
using BrasileirinhoRestourant.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

// Syncfusion Community License: defina a chave em appsettings ou User Secrets como "Syncfusion:LicenseKey".
// Sem chave o componente renderiza com um aviso de licença, mas continua funcional para dev.
var syncfusionKey = builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrWhiteSpace(syncfusionKey))
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionKey);

// MaximumReceiveMessageSize: o default do Blazor Server é 32 KB, insuficiente para uploads.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 20 * 1024 * 1024;
    });

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");

// Factory cria DbContext fresco por operação — evita conflito de rastreio
// no circuit longo do Blazor Server.
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(IServicoBase<>), typeof(ServicoBase<>));
builder.Services.AddScoped<IServicoVenda, ServicoVenda>();
builder.Services.AddScoped<IServicoContasPagar, ServicoContasPagar>();
builder.Services.AddScoped<IServicoContasReceber, ServicoContasReceber>();
builder.Services.AddScoped<IServicoFluxoCaixa, ServicoFluxoCaixa>();
builder.Services.AddScoped<IArmazenamentoFotos, ArmazenamentoFotos>();
builder.Services.AddScoped<IArmazenamentoNotas, ArmazenamentoNotas>();
builder.Services.AddSyncfusionBlazor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
