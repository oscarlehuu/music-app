using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using server.Interface;

namespace server.Database
{
    public class CreateTableMusic : IDynamoDBMigration
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly DynamoDBUtils _dynamoDBUtils;
        public CreateTableMusic(AmazonDynamoDBClient client, DynamoDBUtils dynamoDBUtils) 
        {
            _client = client;
            _dynamoDBUtils = dynamoDBUtils;
        }

        public async Task<bool> ShouldExecuteAsync() {
            try
            {
                Console.WriteLine("Checking if 'music' table exists...");
                await _client.DescribeTableAsync(new DescribeTableRequest("music"));
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
            Console.WriteLine("CreateTableLogin.ExecuteAsync called");
            string tableName = "music";
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