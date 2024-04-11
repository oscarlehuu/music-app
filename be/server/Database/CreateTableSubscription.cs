using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using server.Interface;

namespace server.Database
{
    public class CreateTableSubscription : IDynamoDBMigration
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly DynamoDBUtils _dynamoDBUtils;

        public CreateTableSubscription(AmazonDynamoDBClient client, DynamoDBUtils dynamoDBUtils)
        {
            _client = client;
            _dynamoDBUtils = dynamoDBUtils;
        }

        public async Task<bool> ShouldExecuteAsync() {
            try
            {
                Console.WriteLine("Checking if 'subscription' table exists...");
                await _client.DescribeTableAsync(new DescribeTableRequest("subscription"));
                Console.WriteLine("Table 'music' exists.");
                return false; // Table exists
            }
            catch (ResourceNotFoundException)
            {
                Console.WriteLine("Table 'music' does not exist.");
                return true; // Table needs to be created
            }
        }
        public async Task ExecuteAsync()
        {
            Console.WriteLine("CreateTableSubscription.ExecuteAsync called");
            string tableName = "subscription";
            List<KeySchemaElement> keySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("userId", KeyType.HASH)
            };
            List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>() 
            {
                new AttributeDefinition("userId", ScalarAttributeType.S),
            };
            ProvisionedThroughput provisionedThroughput = new ProvisionedThroughput(3, 3);

            await _dynamoDBUtils.CreateTableIfNotExisted(tableName, attributeDefinitions, keySchema, provisionedThroughput);
        }
    }
}