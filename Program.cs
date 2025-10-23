using Microsoft.EntityFrameworkCore;
using VirtualCard.TokenResponses;
using VirtualCard.Helper;
using VirtualCard.Interface;
using VirtualCard.Services;
using VirtualCard.Help;
using VirtualCard.Data;
using VirtualCard.Model;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;
using VirtualCard.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connection = builder.Configuration.GetConnectionString("TestDb");
builder.Services.AddDbContext<VirtualCardDbContext>(options => options.UseSqlServer(connection));

var connect = builder.Configuration.GetConnectionString("UserDetails");
builder.Services.AddDbContext<UserProfileDbContext>(options => options.UseSqlServer(connect));

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

string encryptionKey = configuration["AppSettings:EncryptionKey"];
string issuerID = configuration["AppSettings:IssuerID"];
string channelID = configuration["AppSettings:ChannelID"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigins",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddScoped<IVirtualCard, VirtualCardServices>();
builder.Services.AddScoped<ICryptoUtils, CryptoUtils>();
builder.Services.AddScoped<IDataEncryption, DataEncryption>();
builder.Services.AddSingleton<CryptoUtils>();

var configurations = builder.Configuration;
SunTrustProxy.AppId = configurations.GetValue<string>("AppSettings:AppId");
SunTrustProxy.InstitutionCode = configurations.GetValue<string>("AppSettings:InstitutionCode");
SunTrustProxy.MiddlewareBaseUrl = configurations.GetValue<string>("AppSettings:MiddleWareUrl");
SunTrustProxy.StbServiceBaseUrl = configurations.GetValue<string>("AppSettings:Stbservice");
SunTrustProxy.AppPassword = configurations.GetValue<string>("AppSettings:AppPassword");

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<GenerateTokens>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
    c.SchemaFilter<VirtualCard.Services.EnumSchemaFilter>();
});

builder.Services.AddHttpClient("GenerateTokens", client =>
{
    client.BaseAddress = new Uri("https://passport-v2.k8.isw.la/passport/");
});

var app = builder.Build();
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

app.UseGlobalExceptionHandling();

app.UseCors("AllowAnyOrigins");
app.UseSwagger();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
