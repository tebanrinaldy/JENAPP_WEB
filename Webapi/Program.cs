using Microsoft.EntityFrameworkCore;
using Webapi;
using Webapi.Data;
using Webapi.Repositories;
using Webapi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

var cadenaconexion = builder.Configuration.GetConnectionString("CadenaConexionDB");
builder.Services.AddDbContext<Connectioncontextdb>(options =>
    options.UseSqlServer(cadenaconexion));

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy
            .AllowAnyOrigin() 
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.UseUrls("http://localhost:5132", "http://192.168.137.96:5132");
builder.Services.AddScoped<JwtTokensGenerator>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<Userservice>();
builder.Services.AddScoped<Saleservice>();
builder.Services.AddScoped<Productservice>();
builder.Services.AddScoped<Inventoryservice>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWTSettings:Key"]))
    }
    );

builder.Services.AddAuthentication();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("PermitirTodo");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
