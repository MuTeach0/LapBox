using System.IO;
using Scalar.AspNetCore;
using DotNetEnv;

using Serilog;
using LapBox.Infrastructure.Data.Persistence;

// Load environment variables from the nearest .env (searches parent folders)
static string FindEnvFile()
{
    var dir = Directory.GetCurrentDirectory();
    while (dir != null)
    {
        var candidate = Path.Combine(dir, ".env");
        if (File.Exists(candidate)) return candidate;
        dir = Directory.GetParent(dir)?.FullName;
    }
    // fallback to default behaviour (current directory)
    return ".env";
}

Env.Load(FindEnvFile());

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// builder.Services.AddCors();

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "LapBox API V1");

        options.EnableDeepLinking();
        options.DisplayRequestDuration();
        options.EnableFilter();
    });

    app.MapScalarApiReference();

    await app.InitializeDatabaseAsync();

    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}
app.UseCoreMiddlewares(builder.Configuration);

app.MapControllers();

app.UseAntiforgery();
app.MapStaticAssets();

// app.MapRazorComponents<App>().AllowAnonymous()
//     .AddInteractiveWebAssemblyRenderMode()
//     .AddAdditionalAssemblies(typeof(LapBox.Client._Imports).Assembly);


app.Run();
