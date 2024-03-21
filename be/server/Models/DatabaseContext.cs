using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace server.Models
{
    public class DatabaseContext : DbContext
    {
        private readonly IConfiguration _configuration;
        // private readonly IMongoClient _client;
        public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration) : base(options) {
            _configuration = configuration;
            //_client = client;
        }
        //public DbSet<Login> Logins { get; set; }
        //public DbSet<Music> Musics { get; set; }
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        //     var connectionString = _configuration.GetConnectionString("MusicAppDb");
        //     //var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);
        //     var databaseName = _configuration.GetConnectionString("DbName");
        //     optionsBuilder.UseMongoDB(connectionString!, databaseName!);
        // }
        // public IMongoDatabase GetDatabase() 
        // {
        // var databaseName = _configuration.GetConnectionString("DbName"); 
        // return _client.GetDatabase(databaseName);
        // }
    }
}