using System.Globalization;
using System.Text.Json.Serialization;
using Dapper;
using Microsoft.Extensions.Options;
using VideoTag.Server.BackgroundServices;
using VideoTag.Server.Configuration;
using VideoTag.Server.Contexts;
using VideoTag.Server.Extensions;
using VideoTag.Server.Hubs;
using VideoTag.Server.OneTimeCommands;
using VideoTag.Server.Repositories;
using VideoTag.Server.Services;
using VideoTag.Server.SqlTypeHandlers;
using VideoTag.Server.StartupCommands;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

SqlMapper.RemoveTypeMap(typeof(DateTime));

SqlMapper.AddTypeHandler(new GuidHandler());
SqlMapper.AddTypeHandler(new DateTimeUtcHandler());

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
builder.Services.AddSingleton<IEnvironmentService, EnvironmentService>();
builder.Services.AddSingleton<IMetaRepository, MetaRepository>();
builder.Services.AddSingleton<VideoLibrarySyncTrigger>();
builder.Services.AddSingleton<IVideoRepository, VideoRepository>();
builder.Services.AddSingleton<IVideoService, VideoService>();
builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<ICategoryService, CategoryService>();
builder.Services.AddSingleton<ITagRepository, TagRepository>();
builder.Services.AddSingleton<ITagService, TagService>();
builder.Services.AddSingleton<ILibraryService, LibraryService>();
builder.Services.AddSingleton<ICustomThumbnailsRepository, CustomThumbnailsRepository>();
builder.Services.AddSingleton<WatchLogRepository>();
builder.Services.AddSingleton<WatchLogService>();
builder.Services.AddScoped<DbMigrationStartupCommand>();
builder.Services.AddScoped<VacuumStartupCommand>();

builder.Services.AddHostedService<RebuildJob>();
builder.Services.AddHostedService<VideoLibrarySync>();

builder.Services.AddSingleton<UpdateMigrationVersionCommand>();

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

app.RunStartupCommand<DbMigrationStartupCommand>();
app.RunStartupCommand<VacuumStartupCommand>();

// Configure the HTTP request pipeline.

app.UseStaticFiles();

app.UseCors();

app.MapHub<SyncHub>("/hub");

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
