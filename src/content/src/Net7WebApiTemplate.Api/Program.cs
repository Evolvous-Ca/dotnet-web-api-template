using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Net7WebApiTemplate.Api.Filters;
using Net7WebApiTemplate.Api.Swagger;
using NLog.Web;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure NLog
var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var swaggerDocOptions = new SwaggerDocOptions();
builder.Configuration.GetSection(nameof(SwaggerDocOptions)).Bind(swaggerDocOptions);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOptions<SwaggerGenOptions>()
    .Configure<IApiVersionDescriptionProvider>((swagger, service) => 
    { 
        foreach (ApiVersionDescription description in service.ApiVersionDescriptions) 
        {
            swagger.SwaggerDoc(description.GroupName, new OpenApiInfo
            { 
                Title = swaggerDocOptions.Title,
                Version = description.ApiVersion.ToString(),
                Description = swaggerDocOptions.Description,
                TermsOfService = new Uri("https://github.com"),
                Contact = new OpenApiContact
                { 
                    Name = swaggerDocOptions.Organization,
                    Email= swaggerDocOptions.Email
                },
                License = new OpenApiLicense
                { 
                    Name = "MIT",
                    Url = new Uri("https://github.com/")
                }
            });
        }

        var security = new Dictionary<string, IEnumerable<string>>
        {
            {"Beaer", Array.Empty<string>()}
        };

        swagger.AddSecurityDefinition("bearer", new OpenApiSecurityScheme 
        {
            Description = "JWT authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        swagger.OperationFilter<AuthorizeCheckOperationFilter>();

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        swagger.IncludeXmlComments(xmlPath);
    
    });
// Register and configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

// Register and configure APi versioning explorer
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
