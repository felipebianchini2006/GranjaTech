using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Infrastructure;
using GranjaTech.Infrastructure.Services.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- In�cio da Se��o de Configura��o de Servi�os ---

builder.Services.AddControllers();

// 1. Pega a string de conex�o do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Adiciona o DbContext ao cont�iner de inje��o de depend�ncia.
builder.Services.AddDbContext<GranjaTechDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. Registra os servi�os da aplica��o.
builder.Services.AddScoped<IGranjaService, GranjaService>();
builder.Services.AddScoped<ILoteService, LoteService>();

// Servi�os para a documenta��o da API (Swagger).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Fim da Se��o de Configura��o de Servi�os ---


var app = builder.Build();

// --- In�cio da Se��o de Configura��o do Pipeline HTTP ---

// Em ambiente de desenvolvimento, habilita a interface do Swagger.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Injeta nosso arquivo CSS customizado para o tema escuro
        c.InjectStylesheet("/css/swagger-dark.css");
    });
}

// Redireciona requisi��es HTTP para HTTPS.
app.UseHttpsRedirection();

// Habilita o servi�o de arquivos est�ticos (da pasta wwwroot)
app.UseStaticFiles();

// Habilita a autoriza��o (usaremos isso mais tarde com login).
app.UseAuthorization();

// Mapeia as rotas definidas nos seus controllers.
app.MapControllers();

// --- Fim da Se��o de Configura��o do Pipeline HTTP ---

// Inicia a aplica��o.
app.Run();