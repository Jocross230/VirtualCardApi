using VirtualCard.TokenResponses;
using VisualCard.Helper;
using VisualCard.Interface;
using VisualCard.Services;


var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSingleton<IVirtualCard, VirtualCardServices>();
builder.Services.AddSingleton<ICryptoUtils, CryptoUtils>();
builder.Services.AddSingleton<CryptoUtils>();

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VisualCard v1");
    });
}
else
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/VisualCard/swagger/v1/swagger.json", "VisualCard v1");
    });
}

app.UseCors("AllowAnyOrigins");
app.UseSwagger();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
