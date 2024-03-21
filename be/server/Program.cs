using Amazon.DynamoDBv2;
using server.Interface;
using server.Database;
using Amazon;
using server.Service;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var awsAccessKey = builder.Configuration.GetValue( "AWS:AcessKey", "");
var awsSecretKey = builder.Configuration.GetValue("AWS:SecretKey", "");
var awsSessionToken = builder.Configuration.GetValue("AWS:SessionToken", "");
var awsRegion = builder.Configuration.GetValue("AWS:Region", "");

builder.Services.AddSingleton<AmazonDynamoDBClient>(new AmazonDynamoDBClient( 
    awsAccessKeyId: awsAccessKey,  
    awsSecretAccessKey: awsSecretKey,
    awsSessionToken: awsSessionToken,
    region: RegionEndpoint.USEast1
));


//builder.Services.AddSingleton<AmazonDynamoDBClient>(); 
builder.Services.AddSingleton<DynamoDBUtils>();
// ------ Database Table ------ //
builder.Services.AddSingleton<CreateTableLogin>();
builder.Services.AddSingleton<CreateTableMusic>();
// ------- --------- ---------/
builder.Services.AddSingleton<ITestDynamoDBConnection, TestDynamoDBConnection>();
// -------- Service -------- //
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddSingleton<MusicService>();
// -------- -------- -------- //
// ... rest of your app setup ...


var app = builder.Build();
// Test connection with database
var testService = app.Services.GetRequiredService<ITestDynamoDBConnection>();
await testService.TestConnection();

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
// ------ ReadAndSaveMusicJsonFile ------ //
var musicService = app.Services.GetRequiredService<MusicService>(); 
await musicService.ReadAndSaveMusicJsonFile();
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
