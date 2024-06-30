using System.Data.Common;
using System.Globalization;
using System.Text.Json.Serialization;
using Dapper;
using EvolveDb;
using VideoTag.Server;
using VideoTag.Server.BackgroundServices;
using VideoTag.Server.Contexts;
using VideoTag.Server.Hubs;
using VideoTag.Server.Repositories;
using VideoTag.Server.Services;
using VideoTag.Server.SqlTypeHandlers;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

SqlMapper.AddTypeHandler(new GuidHandler());

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSignalR();

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<LibraryConfiguration>(_ => new LibraryConfiguration
{
    LibraryPath = builder.Configuration.GetValue<string>(LibraryConfiguration.LibraryPathKey) ??
                  throw new Exception($"Missing configuration value for `{LibraryConfiguration.LibraryPathKey}"),
    AllowedFileExtensions = builder.Configuration.GetSection(LibraryConfiguration.AllowedFileExtensionsKey).Get<List<string>>() ??
                            throw new Exception($"Missing configuration value for `{LibraryConfiguration.AllowedFileExtensionsKey}`"),
    ThumbnailDirectoryPath = Path.Combine(builder.Environment.WebRootPath, "images")
});
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

builder.Configuration.AddJsonFile("localsettings.json");

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var dapperContext = serviceProvider.GetRequiredService<DapperContext>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    using (var connection = dapperContext.CreateConnection())
    {
        var evolve = new Evolve((DbConnection)connection, msg => logger.LogInformation("{Message}", msg))
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
