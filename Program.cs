using Gym.Data;
using Gym.Models;
using Gym.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurare Serilog pentru loguri detaliate
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
builder.Host.UseSerilog();

// 2. Conectare Bază de Date
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 4. CONFIGURARE JWT - REPARATĂ PENTRU A EVITA EROAREA IDX10517
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        // ACEASTĂ CHEIE TREBUIE SĂ FIE IDENTICĂ CU CEA DIN USERSCONTROLLER
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("CheiaMeaSuperSecretaGlowGym202612345678")),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    // 🌟 FORȚĂM VALIDATORUL SĂ NU CEARĂ "kid" (Key ID)
    options.SecurityTokenValidators.Clear();
    options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler());
});

// 5. Configurare OData
static Microsoft.OData.Edm.IEdmModel GetEdmModel()
{
    var mb = new ODataConventionModelBuilder();
    mb.EntitySet<User>("Users");
    mb.EntitySet<GymClass>("GymClasses");
    mb.EntitySet<GymRoom>("GymRooms");
    mb.EntitySet<SportType>("SportTypes");
    mb.EntitySet<Booking>("Bookings");
    return mb.GetEdmModel();
}

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .AddOData(opt => opt.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100)
        .AddRouteComponents("odata", GetEdmModel()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 6. Servicii pentru Multi-tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication(); // 🌟 Foarte important să fie în această ordine
app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:5218");