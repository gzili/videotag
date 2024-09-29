using System.Data.Common;
using System.Globalization;
using System.Text.Json.Serialization;
using Dapper;
using EvolveDb;
using Microsoft.Extensions.Options;
using VideoTag.Server.BackgroundServices;
using VideoTag.Server.Configuration;
using VideoTag.Server.Contexts;
using VideoTag.Server.Hubs;
using VideoTag.Server.Repositories;
using VideoTag.Server.Services;
using VideoTag.Server.SqlTypeHandlers;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

SqlMapper.AddTypeHandler(new GuidHandler());

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("syncsettings.json");

// Add services to the container.
builder.Services.AddOptions<SyncOptions>()
    .Bind(builder.Configuration.GetSection(SyncOptions.Sync))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<SyncOptions>, SyncOptionsValidation>();

builder.Services.AddSignalR();

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<VideoLibrarySyncTrigger>();
builder.Services.AddSingleton<IVideoRepository, VideoRepository>();
builder.Services.AddSingleton<VideoService>();
builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<ICategoryService, CategoryService>();
builder.Services.AddSingleton<ITagRepository, TagRepository>();
builder.Services.AddSingleton<ITagService, TagService>();

builder.Services.AddHostedService<VideoLibrarySync>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin();
        policyBuilder.AllowAnyMethod();
        policyBuilder.AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var dapperContext = serviceProvider.GetRequiredService<DapperContext>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    using (var connection = dapperContext.CreateConnection())
    {
        var evolve = new Evolve(
            (DbConnection)connection,
            msg => logger.LogInformation("{Message}", msg)
        )
        {
            Locations = new[] { "Migrations" },
            MetadataTableName = "Migrations",
            IsEraseDisabled = true
        };
        
        evolve.Migrate();
    }
}

// Configure the HTTP request pipeline.

app.UseStaticFiles();

app.UseCors();

app.MapHub<SyncHub>("/hub");

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
