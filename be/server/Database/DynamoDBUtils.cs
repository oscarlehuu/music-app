using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace server.Database
{
    public class DynamoDBUtils
    {
        private readonly AmazonDynamoDBClient _client;
        public DynamoDBUtils(AmazonDynamoDBClient client) 
        {
            _client = client;
        }
        public async Task CreateTableIfNotExisted(
            string tableName, List<AttributeDefinition> attributeDefinitions, 
            List<KeySchemaElement> keySchema, ProvisionedThroughput provisionedThroughput) 
        {
            try 
            {
                Console.WriteLine($"Attempting to create table '{tableName}'");
                await _client.CreateTableAsync(new CreateTableRequest 
                {
                    TableName = tableName,
                    AttributeDefinitions = attributeDefinitions,
                    KeySchema = keySchema,
                    ProvisionedThroughput = provisionedThroughput
                });
                Console.WriteLine($"Table '{tableName}' created successfully.");
            } catch (ResourceInUseException) 
            {
                Console.WriteLine($"Table '{tableName}' already exists.");
            } catch (Exception ex)
            {
                Console.WriteLine($"Error creating table: {ex.Message}"); 
            }
        }
    }
}