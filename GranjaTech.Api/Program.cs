using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Infrastructure;
using GranjaTech.Infrastructure.Services.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Início da Seção de Configuração de Serviços ---

builder.Services.AddControllers();

// 1. Pega a string de conexão do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Adiciona o DbContext ao contêiner de injeção de dependência.
builder.Services.AddDbContext<GranjaTechDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. Registra os serviços da aplicação.
builder.Services.AddScoped<IGranjaService, GranjaService>();
builder.Services.AddScoped<ILoteService, LoteService>();

// Serviços para a documentação da API (Swagger).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Fim da Seção de Configuração de Serviços ---


var app = builder.Build();

// --- Início da Seção de Configuração do Pipeline HTTP ---

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

// Redireciona requisições HTTP para HTTPS.
app.UseHttpsRedirection();

// Habilita o serviço de arquivos estáticos (da pasta wwwroot)
app.UseStaticFiles();

// Habilita a autorização (usaremos isso mais tarde com login).
app.UseAuthorization();

// Mapeia as rotas definidas nos seus controllers.
app.MapControllers();

// --- Fim da Seção de Configuração do Pipeline HTTP ---

// Inicia a aplicação.
app.Run();