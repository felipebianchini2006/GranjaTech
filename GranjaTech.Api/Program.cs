using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Infrastructure;
using GranjaTech.Infrastructure.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization; // Adicione este using

var builder = WebApplication.CreateBuilder(args);

// --- In�cio da Sec��o de Configura��o de Servi�os ---

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
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

// Registo dos Servi�os
builder.Services.AddScoped<IGranjaService, GranjaService>();
builder.Services.AddScoped<ILoteService, LoteService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFinancasService, FinancasService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GranjaTech API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Description = "Insira o token JWT desta forma: Bearer {seu token}", Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } } });
});

// --- Fim da Sec��o de Configura��o de Servi�os ---

var app = builder.Build();

// --- In�cio da Sec��o de Configura��o do Pipeline HTTP ---

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
        
        // Verificar se a resposta já foi iniciada
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { 
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.InjectStylesheet("/css/swagger-dark.css");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// --- Fim da Sec��o de Configura��o do Pipeline HTTP ---

app.Run();
