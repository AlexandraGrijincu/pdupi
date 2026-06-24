using Gym.Models;
using Gym.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Serilog;
using System.Text.Json.Serialization;
// 🌟 Namespace-uri adăugate pentru validarea securizată a token-urilor JWT
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 🌟 CONFIGURAREA SERVICIULUI DE AUTENTIFICARE JWT
// Serverul decodează automat headerul "Authorization: Bearer <token>" primit de la telefon
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        // Folosește la bit aceeași cheie secretă setată în UsersController!
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("CheiaMeaSuperSecretaGlowGym2026!!!🌸")),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// CONFIGURAREA MODELULUI ODATA PENTRU TABELE
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

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAll");

// 🌟 ORDINE CRITICĂ .NET: Întâi citim cine ești (Authentication), apoi ce ai voie să faci (Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:5218");