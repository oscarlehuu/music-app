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

builder.Services.AddCors(options => {
    options.AddPolicy("AllowSpecificOrigin", 
                        builder => builder.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod()
                      );
});

var awsAccessKey = builder.Configuration.GetValue( "AWS:AcessKey", "");
var awsSecretKey = builder.Configuration.GetValue("AWS:SecretKey", "");
var awsSessionToken = builder.Configuration.GetValue("AWS:SessionToken", "");
var awsRegion = builder.Configuration.GetValue("AWS:Region", "");

var jwtKey = builder.Configuration.GetValue("JWT:Key", "");
var jwtIssuer = builder.Configuration.GetValue("JWT:Issuer", "");
var jwtAudience = builder.Configuration.GetValue("JWT:Audience", "");

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
            ValidIssuer = jwtIssuer,
            ValidateIssuer = true,
            ValidAudience = jwtAudience,
            ValidateAudience = true,
            ValidateLifetime = true
        };
    }
);

//builder.Services.AddSingleton<AmazonDynamoDBClient>(); 
builder.Services.AddSingleton<DynamoDBUtils>();
// ------ Database Table ------ //
builder.Services.AddSingleton<CreateTableLogin>();
builder.Services.AddSingleton<CreateTableMusic>();
builder.Services.AddSingleton<CreateTableSubscription>();
builder.Services.AddSingleton<UpdateGSITableLogin>();
// ------- --------- ---------/
builder.Services.AddSingleton<ITestDynamoDBConnection, TestDynamoDBConnection>();
builder.Services.AddSingleton<S3BucketService>();
// -------- Service -------- //
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<LoginController>();
builder.Services.AddSingleton<MusicService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<SubscriptionController>();
// -------- -------- -------- //
// ... rest of your app setup ...


var app = builder.Build();
// app.Use(async (context, next) =>
// {
//     Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path} -  Has Route Values: {context.Request.RouteValues.Any()} - Values: {string.Join(", ", context.Request.RouteValues.Select(r => $"{r.Key}: {r.Value}"))}"); 

//     // Add this: Log the raw route template  and action name
//     var routeData = context.GetRouteData();
//     Console.WriteLine($"Matched Action Name: {routeData?.Values["action"]}");

//     await next();
// });
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

// Create table Subscription
var tableSubscriptionMigration = app.Services.GetServices<CreateTableSubscription>();
foreach (var migration in tableSubscriptionMigration)
{
    Console.WriteLine($"Processing migration: {migration.GetType().Name}");
    if (await migration.ShouldExecuteAsync())
    {
        await migration.ExecuteAsync();

    }
}
// ------ -------- -------- ------ //
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
