using LiBookerWebApi.Infrastructure;
using LiBookerWebApi.Endpoints;
using LiBookerWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

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
}

app.Run();
