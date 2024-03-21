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
        [DynamoDBProperty("web_url")]
        public string webUrl { get; set; }
        [DynamoDBProperty("img_url")]
        public string imgUrl { get; set; }
    }
}