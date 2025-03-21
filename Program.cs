using Microsoft.EntityFrameworkCore;
using VirtualCard.TokenResponses;
using VisualCard.Helper;
using VisualCard.Interface;
using VisualCard.Services;
using VirtualCard.Data;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigins",
        builder => builder
            .AllowAnyOrigin() // Allow requests from any origin
            .AllowAnyMethod() // Allow any HTTP method
            .AllowAnyHeader()); // Allow any header
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Register the database context
var connection = builder.Configuration.GetConnectionString("TestDb");
builder.Services.AddDbContext<VirtualCardDbContext>(options =>
    options.UseSqlServer(connection));

// Read configuration first
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Get the encryption key from the configuration
string encryptionKey = configuration["AppSettings:EncryptionKey"];
string issuerID = configuration["AppSettings:IssuerID"];
string channelID = configuration["AppSettings:ChannelID"];

//string authToken = configuration["AppSettings:AuthToken"];

// Register services with DI
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigins",
        builder => builder
            .AllowAnyOrigin() // Allow requests from any origin
            .AllowAnyMethod() // Allow any HTTP method
            .AllowAnyHeader());// Allow any header
});



// Register the other services
builder.Services.AddScoped<IVirtualCard, VirtualCardServices>();
builder.Services.AddScoped<ICryptoUtils, CryptoUtils>();
builder.Services.AddSingleton<CryptoUtils>();

var configurations = builder.Configuration;
SunTrustProxy.AppId = configurations.GetValue<string>("AppSettings:AppId");
SunTrustProxy.InstitutionCode = configurations.GetValue<string>("AppSettings:InstitutionCode");
SunTrustProxy.MiddlewareBaseUrl = configurations.GetValue<string>("AppSettings:MiddleWareUrl");
SunTrustProxy.StbServiceBaseUrl = configurations.GetValue<string>("AppSettings:Stbservice");
SunTrustProxy.AppPassword = configurations.GetValue<string>("AppSettings:AppPassword");

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<GenerateTokens>();

// Add Swagger and related services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddHttpClient("GenerateTokens", client =>
{
    client.BaseAddress = new Uri("https://passport-v2.k8.isw.la/passport/");
});


var app = builder.Build();

// Configure the HTTP request pipeline for Swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VirtualCard v1");
    });
}
else
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/VirtualCard/swagger/v1/swagger.json", "VirtualCard v1");
    });
}

app.UseCors("AllowAnyOrigins");
app.UseSwagger();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
