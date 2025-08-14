using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApplication1.Models;
using WebApplication1.Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<InventoryManagementContext>();

var jwtkey = builder.Configuration["Jwt:Key"];
var jwtaudience = builder.Configuration["Jwt:Audience"];
var jwtissuer = builder.Configuration["Jwt:Issuer"];
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateAudience  = true,
          ValidateIssuer = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          
          ValidAudience=jwtaudience,
          ValidIssuer=jwtissuer,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtkey))
        };
    })
    ;
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IRequestOrderService,RequestOrderService>();
builder.Services.AddScoped<IItemMasterService,ItemMasterService>();
builder.Services.AddScoped<IProductionOrderService,ProductionService>();
builder.Services.AddScoped<IDeliveryService,DeliveryService>();


builder.Services.AddSwaggerGen(s =>
{
    s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Masukkan token"

    });
    s.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },Array.Empty<string>()
        }
    });

});
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();


app.MapControllers();
if (OperatingSystem.IsWindows())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.Run();
