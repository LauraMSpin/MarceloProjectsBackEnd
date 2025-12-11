using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using cronograma_atividades_backend.Data;
using cronograma_atividades_backend.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configurar EF Core com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// Configurar CORS para permitir requisições do frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Aplicar migrations automaticamente e criar usuário admin padrão
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    
    db.Database.Migrate();
    
    // Criar usuário admin padrão se não existir
    if (!db.Usuarios.Any(u => u.Role == "Admin"))
    {
        var adminLogin = config["AdminPadrao:Login"] ?? "admin";
        var adminSenha = config["AdminPadrao:Senha"] ?? "admin123";
        var adminNome = config["AdminPadrao:Nome"] ?? "Administrador";
        var adminEmail = config["AdminPadrao:Email"] ?? "admin@sistema.com";
        
        using var sha256 = SHA256.Create();
        var senhaHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(adminSenha)));
        
        var admin = new Usuario
        {
            Id = Guid.NewGuid(),
            Login = adminLogin,
            SenhaHash = senhaHash,
            Nome = adminNome,
            Email = adminEmail,
            Role = "Admin",
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };
        
        db.Usuarios.Add(admin);
        db.SaveChanges();
        
        Console.WriteLine($"Usuário admin criado: {adminLogin}");
    }
}

app.Run();

