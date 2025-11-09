using Webapi.Data;
using Microsoft.EntityFrameworkCore;
using Webapi.Repositories;
using Webapi.Services;

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

builder.WebHost.UseUrls("http://localhost:5132", "http://192.168.1.16:5132");
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<Userservice>();
builder.Services.AddScoped<Saleservice>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("PermitirTodo");

app.UseAuthorization();

app.MapControllers();

app.Run();
