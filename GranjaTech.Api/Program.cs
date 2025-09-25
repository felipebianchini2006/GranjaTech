using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Infrastructure;
using GranjaTech.Infrastructure.Services;
using GranjaTech.Infrastructure.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// --- Início da Seção de Configuração de Serviços ---

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Lê AllowedOrigins da configuração (ex.: "https://meu-swa.azurestaticapps.net;http://localhost:3000")
// Configure essa chave em App Service -> Configuration (ou em appsettings.Production.json)
var allowedOriginsConfig = builder.Configuration.GetValue<string>("AllowedOrigins");

// Sempre inclua localhost:3000 como fallback em desenvolvimento
var defaultDevOrigins = new[] { "http://localhost:3000" };

// Parse das origens ou usa fallback
string[] allowedOrigins;
if (!string.IsNullOrWhiteSpace(allowedOriginsConfig))
{
    allowedOrigins = allowedOriginsConfig
        .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim())
        .ToArray();
}
else
{
    allowedOrigins = defaultDevOrigins;
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(allowedOrigins)
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                          // .AllowCredentials(); // habilite se precisar enviar cookies/credenciais via CORS
                      });
});

// Configuração otimizada do serializador JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = false; // Reduzir tamanho da resposta
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.MaxDepth = 64; // Limitar profundidade para evitar loops infinitos
});

builder.Services.AddHttpContextAccessor();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GranjaTechDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registo dos Serviços
builder.Services.AddScoped<IGranjaService, GranjaService>();
builder.Services.AddScoped<ILoteService, LoteService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFinancasService, FinancasService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IRelatorioAvancadoService, RelatorioAvancadoService>();
builder.Services.AddScoped<IAviculturaService, AviculturaService>();
builder.Services.AddScoped<GranjaTech.Infrastructure.Services.Interfaces.ICacheService, GranjaTech.Infrastructure.Services.Implementations.MemoryCacheService>();

// Adicionar Memory Cache
builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };

    // --- ALTERAÇÃO APLICADA AQUI ---
    // Validação robusta da chave JWT para evitar que a aplicação trave na inicialização.
    var jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey))
    {
        // Se a chave não estiver configurada, a aplicação irá parar com um erro claro.
        throw new InvalidOperationException("A chave JWT (Jwt:Key) não está configurada.");
    }
    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    // --- FIM DA ALTERAÇÃO ---
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GranjaTech API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Insira o token JWT desta forma: Bearer {seu token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    });
});

// --- Fim da Seção de Configuração de Serviços ---

var app = builder.Build();

// Lê flag para habilitar Swagger em produção via configuração (Swagger__Enabled=true)
var enableSwagger = app.Configuration.GetValue<bool>("Swagger:Enabled");

// --- Início da Seção de Configuração do Pipeline HTTP ---

// Middleware de logging de requisições
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestTracking");
    var requestId = Guid.NewGuid().ToString("N")[..8];

    logger.LogInformation("Iniciando requisição {RequestId}: {Method} {Path}", requestId, context.Request.Method, context.Request.Path);

    try
    {
        await next();
        logger.LogInformation("Finalizando requisição {RequestId} com status {StatusCode}", requestId, context.Response.StatusCode);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Exceção na requisição {RequestId}: {Method} {Path}", requestId, context.Request.Method, context.Request.Path);

        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Erro interno do servidor",
                requestId = requestId,
                path = context.Request.Path.Value
            });
        }
        else
        {
            logger.LogWarning("Não foi possível retornar erro JSON para requisição {RequestId} pois a resposta já havia sido iniciada", requestId);
        }
    }
});

// Swagger habilitado em Desenvolvimento OU quando Swagger:Enabled=true nas configurações
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.InjectStylesheet("/css/swagger-dark.css");
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GranjaTech API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Ativa CORS com as origens lidas da configuração
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Endpoint de health sem autenticação (útil para Health Check do App Service)
app.MapGet("/health", () => Results.Ok("OK")).AllowAnonymous();

// Seed de dados de avicultura em desenvolvimento
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<GranjaTechDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // Aplicar migrações pendentes
            context.Database.Migrate();

            // Executar seed de dados de avicultura
            GranjaTech.Infrastructure.Data.AviculturaSeedData.SeedAviculturaData(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro durante a inicialização do banco de dados");
        }
    }
}

// --- Fim da Seção de Configuração do Pipeline HTTP ---

app.Run();
