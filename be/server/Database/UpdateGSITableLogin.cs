using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace server.Database
{
    public class UpdateGSITableLogin
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly DynamoDBUtils _dynamoDBUtils;
        private readonly string _tableName = "login";
        public UpdateGSITableLogin(AmazonDynamoDBClient client, DynamoDBUtils dynamoDBUtils)
        {
            _client = client;
            _dynamoDBUtils = dynamoDBUtils;
        }
        public async Task<bool> EmailIndexExists(string indexName)
        {
            var describeTableRequest = new DescribeTableRequest()
            {
                TableName = _tableName
            };
            var describeResponse = await _client.DescribeTableAsync(describeTableRequest);
            var globalSecondaryIndexes = describeResponse.Table.GlobalSecondaryIndexes;
            return globalSecondaryIndexes != null && globalSecondaryIndexes.Any(index => index.IndexName == indexName);
        }
        public async Task UpdateGSI() 
        {
            var updateTableRequest = new UpdateTableRequest() 
            {
                TableName = _tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition { AttributeName = "email", AttributeType = "S" },
                },
                GlobalSecondaryIndexUpdates = new List<GlobalSecondaryIndexUpdate>
                {
                    new GlobalSecondaryIndexUpdate 
                    {
                        Create = new CreateGlobalSecondaryIndexAction
                        {
                            IndexName = "email-index",
                            KeySchema = new List<KeySchemaElement>
                            {
                                new KeySchemaElement
                                {
                                    AttributeName = "email",
                                    KeyType = "HASH",
                                }
                            },
                            Projection = new Projection
                            {
                                ProjectionType = "ALL",
                            },
                            ProvisionedThroughput = new ProvisionedThroughput
                            {
                                ReadCapacityUnits = 5,
                                WriteCapacityUnits = 5,
                            }
                        }
                    }
                }
            };

            await _client.UpdateTableAsync(updateTableRequest);
        }
    }
}