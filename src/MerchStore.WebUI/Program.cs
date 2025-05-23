using System.Reflection;
using System.Text.Json.Serialization;
using MerchStore.Application;
using MerchStore.Infrastructure;
using MerchStore.WebUI.Authentication.ApiKey;
using MerchStore.WebUI.Infrastructure;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using DotNetEnv;


DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// MVC + JSON options
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy();
        options.JsonSerializerOptions.DictionaryKeyPolicy = new JsonSnakeCaseNamingPolicy();
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Cookie auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

// API Key auth & policy
builder.Services.AddAuthentication()
   .AddApiKey(builder.Configuration["ApiKey:Value"] 
       ?? throw new InvalidOperationException("API Key is not configured in the application settings."));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiKeyPolicy", policy =>
        policy.AddAuthenticationSchemes(ApiKeyAuthenticationDefaults.AuthenticationScheme)
              .RequireAuthenticatedUser());
});

// Session
builder.Services.AddSession();

// Application & Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "MerchStore API",
        Version     = "v1",
        Description = "API for MerchStore product catalog",
        Contact     = new OpenApiContact
        {
            Name  = "MerchStore Support",
            Email = "support@merchstore.example.com"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition(ApiKeyAuthenticationDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = "API Key Authentication. Enter your API key in the field below.",
        Name        = ApiKeyAuthenticationDefaults.HeaderName,
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = ApiKeyAuthenticationDefaults.AuthenticationScheme
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        cors => cors
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Seed test data in dev
    //app.Services.SeedDatabaseAsync().Wait();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MerchStore API V1");
    options.RoutePrefix = "swagger";
});

app.UseRouting();

// Session middleware (before MVC endpoints)
app.UseSession();

// CORS + Auth
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();

// MVC routing
app.MapControllerRoute(
    name:    "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
// in Program.cs or Startup.cs
app.UseStaticFiles();
app.UseStaticFiles();