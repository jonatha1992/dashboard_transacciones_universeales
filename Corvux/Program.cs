using Microsoft.EntityFrameworkCore;
using Corvux.Data;
using Corvux.Services;
using Corvux.Components;

var builder = WebApplication.CreateBuilder(args);

// EF Core con IDbContextFactory — correcto para Blazor Server
builder.Services.AddDbContextFactory<ComplianceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ComplianceDb")));

// Servicios de la aplicación
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<ComplianceRuleEngine>();
builder.Services.AddScoped<TransactionImportParser>();

// Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Crear la base de datos si no existe
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ComplianceDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
