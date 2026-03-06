using Api;

using Application.Common;
using Application.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddAzureWebAppDiagnostics();

// Register the Swagger generator, defining 1 or more Swagger documents
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerOpenAPI();

builder.Services.AddProblemDetails();

builder.Services.AddCommonServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

// Configure the HTTP request pipeline.
var app = await builder.Build().ConfigurePipelineAsync();

app.Run();

public partial class Program { }