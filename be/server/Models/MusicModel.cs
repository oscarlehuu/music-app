using Amazon.DynamoDBv2.DataModel;

namespace server.Models
{
    [DynamoDBTable("music")]
    public class MusicModel
    { 
        [DynamoDBHashKey]
        public string Id { get; set; }
        [DynamoDBProperty("title")]
        public string title { get; set; }
        [DynamoDBProperty("artist")]
        public string artist { get; set; }
        [DynamoDBProperty("year")]
        public string year { get; set; }
        [DynamoDBProperty("web_url")]
        public string web_url { get; set; }
        [DynamoDBProperty("img_url")]
        public string img_url { get; set; }
    }
}