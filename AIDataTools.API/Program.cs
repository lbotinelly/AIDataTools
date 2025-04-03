using AIDataTools.API.Extensions;
using AIDataTools.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDiagnostics();

// Register services
builder.Services.AddSingleton<ILlmService, LlmService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles(); // Add this line to serve default files like index.html
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Log all registered controllers
var controllerTypes = Assembly.GetExecutingAssembly().GetTypes()
    .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
    .Select(type => type.Name);

Console.WriteLine("Registered Controllers:");
foreach (var controller in controllerTypes)
{
    Console.WriteLine(controller);
}

// Log all endpoints
var endpoints = GetEndpoints();
Console.WriteLine("Registered Endpoints:");
foreach (var endpoint in endpoints)
{
    Console.WriteLine($"{endpoint.HttpMethod} {endpoint.Url}");
}

app.Run();

IEnumerable<(string HttpMethod, string Url)> GetEndpoints()
{
    var controllerTypes = Assembly.GetExecutingAssembly().GetTypes()
        .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);

    foreach (var controllerType in controllerTypes)
    {
        var methods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        foreach (var method in methods)
        {
            var httpMethodAttributes = method.GetCustomAttributes()
                .Where(attr => attr is HttpMethodAttribute)
                .Cast<HttpMethodAttribute>();

            foreach (var attr in httpMethodAttributes)
            {
                var template = attr.Template ?? string.Empty;
                var httpMethod = attr.HttpMethods.First();
                var controllerRoute = controllerType.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty;
                var controllerName = controllerType.Name.Replace("Controller", string.Empty);
                var url = $"/{controllerRoute}/{template}".Replace("[controller]", controllerName).Trim('/');

                yield return (httpMethod, url);
            }
        }
    }
}
