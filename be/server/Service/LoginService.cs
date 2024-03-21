// // using System;
// // using System.Collections.Generic;
// // using System.Linq;
// // using System.Text;
// // using System.Threading.Tasks;
// // using server.Interface;
// // using server.Models;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using server.Database;
using server.Interface;
using server.Models;

namespace server.Service
{
    public class LoginService : ILoginService
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly CreateTableLogin _createTableLogin;
        public LoginService(AmazonDynamoDBClient client, CreateTableLogin createTableLogin)
        {
            _client = client;
            _createTableLogin = createTableLogin;
        } 
        private async Task<bool> TableHasDataAsync(string tableName)
        {
            var scanRequest = new ScanRequest(tableName);
            var response = await _client.ScanAsync(scanRequest);
            return response.Count > 0;
        }

        public async Task InsertLoginAsync(Login login) 
        {
            var request = new PutItemRequest
            {
                TableName = "login",
                Item = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue { S = login.Id }},
                    {"email", new AttributeValue { S = login.email }},
                    {"user_name", new AttributeValue { S = login.userName }},
                    {"password", new AttributeValue { S = login.password }},
                }
            };
            await _client.PutItemAsync(request);
        }

        // Create a “login” table in DynamoDB containing 10 entities with the following attributes and values. 
        // Note: This task can be done via code or console.
        public async Task InsertLoginTableData() 
        {
            bool hasData = await TableHasDataAsync("login");
            if (hasData)
            {
              Console.WriteLine("Table 'login' already populated. Skipping.");
              return; // Exit if table exists and has data
            }
            string firstName = "Nghia";
            string lastName = "Le";
            string studentId = "3654028";
            int loginQuantity = 10;
            string password = "012345";
            for (int i = 0; i < loginQuantity; i++) 
            {
                string email = studentId + i + "@rmit.edu.au";
                string userName = firstName + lastName + i;
                var newLogin = new Login {
                    Id = Guid.NewGuid().ToString(),
                    email = email,
                    userName = userName,
                    password = password
                };
                await InsertLoginAsync(newLogin);  
                password = password.Substring(1) + ((password[5] - '0' + 1) % 10);
            }
        }
    }
}