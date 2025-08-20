using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Infrastructure;
using GranjaTech.Infrastructure.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Início da Seção de Configuração de Serviços ---

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

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GranjaTechDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IGranjaService, GranjaService>();
builder.Services.AddScoped<ILoteService, LoteService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFinancasService, FinancasService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>(); // Confirme que esta linha existe
builder.Services.AddScoped<IEstoqueService, EstoqueService>();



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

// --- INÍCIO DA CORREÇÃO NO SWAGGER ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GranjaTech API", Version = "v1" });

    // A configuração foi alterada para usar o tipo 'Http' com o esquema 'Bearer'
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta forma: Bearer {seu token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, // Alterado de ApiKey para Http
        Scheme = "bearer", // Recomenda-se minúsculo por convenção
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// --- FIM DA CORREÇÃO NO SWAGGER ---

var app = builder.Build();

// --- Início da Seção de Configuração do Pipeline HTTP ---

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

// A ordem aqui é crucial: Autenticação primeiro, depois Autorização.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- Fim da Seção de Configuração do Pipeline HTTP ---

app.Run();