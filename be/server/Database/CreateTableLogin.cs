using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using server.Interface;

namespace server.Database
{
    public class CreateTableLogin: IDynamoDBMigration
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly DynamoDBUtils _dynamoDBUtils;
        public CreateTableLogin(AmazonDynamoDBClient client, DynamoDBUtils dynamoDBUtils) 
        {
            _client = client;
            _dynamoDBUtils = dynamoDBUtils;
        }
        public async Task<bool> ShouldExecuteAsync() {
            try
            {
                Console.WriteLine("Checking if 'login' table exists...");
                await _client.DescribeTableAsync(new DescribeTableRequest("login"));
                Console.WriteLine("Table 'login' exists.");
                return false; // Table exists
            }
            catch (ResourceNotFoundException)
            {
                Console.WriteLine("Table 'login' does not exist.");
                return true; // Table needs to be created
            }
        }
        public async Task ExecuteAsync()
        {
            Console.WriteLine("CreateTableLogin.ExecuteAsync called");
            string tableName = "login";
            List<KeySchemaElement> keySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("id", KeyType.HASH)
            };
            List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>() 
            {
                new AttributeDefinition("id", ScalarAttributeType.S),
            };
            ProvisionedThroughput provisionedThroughput = new ProvisionedThroughput(3, 3);

            await _dynamoDBUtils.CreateTableIfNotExisted(tableName, attributeDefinitions, keySchema, provisionedThroughput);
        }
    }
}