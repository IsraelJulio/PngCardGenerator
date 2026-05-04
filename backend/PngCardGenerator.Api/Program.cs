using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PngCardGenerator.Api.Data;
using PngCardGenerator.Api.Rendering;
using PngCardGenerator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CardGeneratorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<FontService>();
builder.Services.AddSingleton<ImageCacheService>();
builder.Services.AddScoped<RenderPipeline>();
builder.Services.AddScoped<CardRendererService>();
builder.Services.AddScoped<CardTemplateService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CardGeneratorDbContext>();
    dbContext.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.MapControllers();

app.Run();
