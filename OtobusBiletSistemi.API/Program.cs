using Microsoft.EntityFrameworkCore;
using OtobusBiletSistemi.Core.Data;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection"), 
        b => b.MigrationsAssembly("OtobusBiletSistemi.API")));

builder.Services.AddScoped<IRepository<Otobus>, Repository<Otobus>>();
builder.Services.AddScoped<IRepository<Yolcu>, Repository<Yolcu>>();
builder.Services.AddScoped<IRepository<Guzergah>, Repository<Guzergah>>();
builder.Services.AddScoped<IRepository<Sefer>, Repository<Sefer>>();
builder.Services.AddScoped<IRepository<Koltuk>, Repository<Koltuk>>();
builder.Services.AddScoped<IRepository<Bilet>, Repository<Bilet>>();
builder.Services.AddScoped<IRepository<Odeme>, Repository<Odeme>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
