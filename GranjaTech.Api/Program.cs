using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Infrastructure;
using GranjaTech.Infrastructure.Services;
using GranjaTech.Infrastructure.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// =============================
// Configurações de CORS
// =============================
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Lê AllowedOrigins da configuração (ex.: "https://seu-swa.azurestaticapps.net;http://localhost:3000")
var allowedOriginsConfig = builder.Configuration.GetValue<string>("AllowedOrigins");

// Fallback para desenvolvimento
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
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
        // Se precisar enviar cookies/credenciais, descomente:
        // .AllowCredentials();
    });
});

// =============================
// MVC / JSON
// =============================
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = false;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.MaxDepth = 64;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// =============================
// EF Core / Npgsql
// =============================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GranjaTechDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// =============================
// DI - Serviços de domínio/aplicação
// =============================
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
builder.Services.AddScoped<GranjaTech.Infrastructure.Services.Interfaces.ICacheService, MemoryCacheService>();

// =============================
// JWT
// =============================
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
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
    };
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

var app = builder.Build();

// =============================
// APLICA MIGRAÇÕES SEMPRE (PROD/DEV)
// =============================
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<GranjaTechDbContext>();
        db.Database.Migrate(); // aplica migrações em qualquer ambiente
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao aplicar migrações no startup");
    }
}

// Flag de configuração para habilitar Swagger em produção
var enableSwagger = app.Configuration.GetValue<bool>("Swagger:Enabled");

// =============================
// Pipeline HTTP
// =============================

// Middleware simples de logging de requisição/resposta
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

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint de health sem autenticação (usado pelo Health Check)
app.MapGet("/health", () => Results.Ok("OK")).AllowAnonymous();

// Rota raiz - redireciona para o Swagger
app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();

// Seed de dados de avicultura EXCLUSIVO para Desenvolvimento
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<GranjaTechDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        // Garantir migrações (já rodou acima), e executar seed apenas em DEV
        GranjaTech.Infrastructure.Data.AviculturaSeedData.SeedAviculturaData(context, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro durante a inicialização/seed de dados (DEV)");
    }
}

app.Run();