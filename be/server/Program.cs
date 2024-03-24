using Amazon.DynamoDBv2;
using server.Interface;
using server.Database;
using Amazon;
using server.Service;
using Amazon.S3;
using Amazon.Util;
using server.Service.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using server.Controllers;
using Amazon.DynamoDBv2.Model;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var awsAccessKey = builder.Configuration.GetValue( "AWS:AcessKey", "");
var awsSecretKey = builder.Configuration.GetValue("AWS:SecretKey", "");
var awsSessionToken = builder.Configuration.GetValue("AWS:SessionToken", "");
var awsRegion = builder.Configuration.GetValue("AWS:Region", "");

var jwtKey = builder.Configuration.GetValue("JWT:Key", "");

builder.Services.AddSingleton<AmazonDynamoDBClient>(new AmazonDynamoDBClient( 
    awsAccessKeyId: awsAccessKey,  
    awsSecretAccessKey: awsSecretKey,
    awsSessionToken: awsSessionToken,
    region: RegionEndpoint.USEast1
));

builder.Services.AddSingleton<AmazonS3Client>(new AmazonS3Client(
    awsAccessKeyId: awsAccessKey,
    awsSecretAccessKey: awsSecretKey,
    awsSessionToken: awsSessionToken,
    region: RegionEndpoint.USEast1
));

// ----- Implementing JWT Token ------ //
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    options => 
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), 
            ValidateIssuer = false, // Adjust based on your needs
            ValidateAudience = false
        };
    }
);

//builder.Services.AddSingleton<AmazonDynamoDBClient>(); 
builder.Services.AddSingleton<DynamoDBUtils>();
// ------ Database Table ------ //
builder.Services.AddSingleton<CreateTableLogin>();
builder.Services.AddSingleton<CreateTableMusic>();
builder.Services.AddSingleton<UpdateGSITableLogin>();
// ------- --------- ---------/
builder.Services.AddSingleton<ITestDynamoDBConnection, TestDynamoDBConnection>();
builder.Services.AddSingleton<S3BucketService>();
// -------- Service -------- //
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<LoginController>();
builder.Services.AddSingleton<MusicService>();
// -------- -------- -------- //
// ... rest of your app setup ...


var app = builder.Build();
// Test connection with database
var testService = app.Services.GetRequiredService<ITestDynamoDBConnection>();
await testService.TestConnection();

// Test connection with S3 Bucket
var s3TestService = app.Services.GetRequiredService<S3BucketService>();
await s3TestService.ListBuckets();

//var migrations = app.Services.GetRequiredService<IEnumerable<IDynamoDBMigration>>();
var tableLoginMigration = app.Services.GetServices<CreateTableLogin>();
// Create table Login
foreach (var migration in tableLoginMigration)
{
    Console.WriteLine($"Processing migration: {migration.GetType().Name}");
    if (await migration.ShouldExecuteAsync())
    {
        await migration.ExecuteAsync();
        
    }
}
var serviceScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = serviceScopeFactory.CreateScope())
{
    var loginService = scope.ServiceProvider.GetRequiredService<ILoginService>(); 
    await loginService.InsertLoginTableData();
}

// Create Table Music
var tableMusicMigration = app.Services.GetServices<CreateTableMusic>();
foreach (var migration in tableMusicMigration)
{
    Console.WriteLine($"Processing migration: {migration.GetType().Name}");
    if (await migration.ShouldExecuteAsync())
    {
        await migration.ExecuteAsync();

    }
}
var updateGSITableLogin = app.Services.GetRequiredService<UpdateGSITableLogin>();
if (!await updateGSITableLogin.EmailIndexExists("email-index"))
{
    Console.WriteLine("Creating email-index...");
    await updateGSITableLogin.UpdateGSI();
} else {
    Console.WriteLine("email-index already exists. Skipping update.");
}
var musicService = app.Services.GetRequiredService<MusicService>(); 
// ------ ReadAndSaveMusicJsonFile ------ //
await musicService.ReadAndSaveMusicJsonFile();
// ------ Download and Upload Images to S3 from a1.json ----- //
await musicService.DownloadAndUploadImages();
// ------ -------- -------- ------ //
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
