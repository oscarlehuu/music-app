// // using System;
// // using System.Collections.Generic;
// // using System.Linq;
// // using System.Text;
// // using System.Threading.Tasks;
// // using server.Interface;
// // using server.Models;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.IdentityModel.Tokens;
using server.Database;
using server.Interface;
using server.Models;

namespace server.Service
{
    public class LoginService : ILoginService
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly string _tableLoginName = "login";
        private readonly IConfiguration _iConfiguration;
        public LoginService(AmazonDynamoDBClient client, IConfiguration iConfiguration)
        {
            _client = client;
            _iConfiguration = iConfiguration;
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
                TableName = _tableLoginName,
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
            bool hasData = await TableHasDataAsync(_tableLoginName);
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
        private async Task<string> GetLoginIdByEmail(string email)
        {
            var queryRequest = new QueryRequest
            {
                TableName = _tableLoginName,
                IndexName = "email-index",
                KeyConditionExpression = "email = :v_email",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":v_email", new AttributeValue { S = email } }
                }
            };
            var response = await _client.QueryAsync(queryRequest);
            if (response.Items.Count == 0) return null;
            return response.Items[0]["id"].S;
        }
        public async Task<string> ValidateLogin(string email, string password) 
        {
            string userId = await GetLoginIdByEmail(email);
            if (userId == null) return null; // Email not found in DynamoDB
            var request = new GetItemRequest 
            {
                TableName = _tableLoginName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "id", new AttributeValue { S = userId } },
                }
            };

            var response = await _client.GetItemAsync(request);
            if (response.Item.Count == 0) return null;

            if (response.Item["password"].S != password) return null; // Password wrong

            var claims = new[] 
            { 
                new Claim(ClaimTypes.Email, email)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_iConfiguration["JWT:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (
                issuer: _iConfiguration["JWT:Issuer"],
                audience: _iConfiguration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}