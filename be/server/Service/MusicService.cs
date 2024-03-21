using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using server.Models;

namespace server.Service
{
    public class MusicService
    {
        private readonly AmazonDynamoDBClient _dynamoDBClient;

        public MusicService(AmazonDynamoDBClient dynamoDBClient)
        {
            _dynamoDBClient = dynamoDBClient;
        }
        public T ReadJsonFile<T>(string filePath)
        {
            // using FileStream fileStream = File.OpenRead(filePath);
            // return JsonSerializer.Deserialize<T>(fileStream);
            using StreamReader reader = new(filePath);
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        }
        private async Task<bool> IsDataAlreadyWritten() 
        {
            string flagFilePath = "../../dataWrittenFlag.txt";
            return File.Exists(flagFilePath);
        }

        public async Task ReadAndSaveMusicJsonFile() 
        {
            if (!await IsDataAlreadyWritten()) 
            {
                //string filePath = "../../a1.json";
                string filePath = "../a1.json";
                string tableName = "music";
                // SongModel songsData = ReadJsonFile<SongModel>(filePath);
                using (StreamReader file = File.OpenText(filePath))
                {
                    SongModel songsData = (SongModel)JsonConvert.DeserializeObject(file.ReadToEnd(), typeof(SongModel));
                    foreach (var song in songsData.Songs) 
                    {
                        var request = new PutItemRequest 
                        {
                            TableName = tableName,
                            Item = new Dictionary<string, AttributeValue>
                            {
                                {"id", new AttributeValue { S = Guid.NewGuid().ToString() }},
                                {"title", new AttributeValue { S = song.title }},
                                {"artist", new AttributeValue { S = song.artist }},
                                { "year", new AttributeValue { S = song.year } },
                                {"web_url", new AttributeValue { S = song.webUrl }},
                                {"img_url", new AttributeValue { S = song.imgUrl }},
                            }
                        };
                        await _dynamoDBClient.PutItemAsync(request);
                        string flagFilePath = "../../dataWrittenFlag.txt"; 
                        File.Create(flagFilePath).Close();
                    }
                }
            }
        }
    }
}