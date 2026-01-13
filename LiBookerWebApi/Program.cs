using LiBookerWebApi.Infrastructure;
using LiBookerWebApi.Endpoints;
using LiBookerWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

if (builder.Environment.IsDevelopment())
{
    // Configure CORS to allow the Blazor WASM origin to make API calls during development
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowLocalBlazor", policy =>
        {
            policy.WithOrigins("https://localhost:7192") // my WASM origin
              .AllowAnyHeader()
              .AllowAnyMethod();
            // .AllowCredentials(); // only if when I need cookies/auth; then don't use AllowAnyOrigin()
        });
    });
}

// existing method that configures Oracle/DbContext
builder.Services.AddOracleDb(builder.Configuration);

// register scoped services
builder.Services.AddScoped<IPersonService, PersonService>();


var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapPersonEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowLocalBlazor");
}

app.Run();
