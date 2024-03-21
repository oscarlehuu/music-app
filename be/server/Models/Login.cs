using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;

namespace server.Models
{
    [DynamoDBTable("login")]
    public class Login
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        [DynamoDBProperty(AttributeName = "email")]
        public required string email { get; set; }
        [DynamoDBProperty(AttributeName = "user_name")]
        public required string userName;
        [DynamoDBProperty(AttributeName = "password")]
        public required string password;
    }
}