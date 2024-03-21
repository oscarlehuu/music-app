using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using server.Interface;

namespace server.Database
{
    public class TestDynamoDBConnection : ITestDynamoDBConnection
    {
        private readonly AmazonDynamoDBClient _client;
        public TestDynamoDBConnection(AmazonDynamoDBClient client) 
        {
            _client = client;
        }
        public async Task TestConnection()
        {
            try
            {
                var response = await _client.ListTablesAsync();
                Console.WriteLine("DynamoDB connection successful. Tables: " + string.Join(", ", response.TableNames));
            }
            catch (Exception ex)
            {
                Console.WriteLine("DynamoDB connection failed: " + ex.Message);
            }
        }
    }
}