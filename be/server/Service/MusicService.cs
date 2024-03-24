using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using server.Models;

namespace server.Service
{
    public class MusicService
    {
        private readonly AmazonDynamoDBClient _dynamoDBClient;
        private readonly AmazonS3Client _s3Client;

        public MusicService(AmazonDynamoDBClient dynamoDBClient, AmazonS3Client s3client)
        {
            _dynamoDBClient = dynamoDBClient;
            _s3Client = s3client;
        }
        // public T ReadJsonFile<T>(string filePath)
        // {
        //     // using FileStream fileStream = File.OpenRead(filePath);
        //     // return JsonSerializer.Deserialize<T>(fileStream);
        //     using StreamReader reader = new(filePath);
        //     return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        // }
        private async Task<bool> IsDataAlreadyWritten() 
        {
            string flagFilePath = "../../dataWrittenFlag.txt";
            return File.Exists(flagFilePath);
        }
        public async Task<bool> IsImageUploadedToS3() 
        {
            string flagFilePath = "../../imageUploaded.txt";
            return File.Exists(flagFilePath);
        }
        //Write a program to automatically load the data from a1.json to your music table.
        //Note: It is acceptable that the program creates the table with only the key attribute(s) in Task 1.2. 
        //The non-key attributes can be created when the program loads the data from the JSON file a1.json in Task 1.3.
        public async Task ReadAndSaveMusicJsonFile() 
        {
            if (!await IsDataAlreadyWritten()) 
            {
                //string filePath = "../../a1.json";
                string filePath = "../../a1.json";
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
        public async Task<byte[]> DownloadImage(string url) {
            using (HttpClient client = new HttpClient()) {
                return await client.GetByteArrayAsync(url);
            }
        }

        public async Task UploadToS3(string bucketName, string key, byte[] image) {
                using (MemoryStream stream = new MemoryStream(image)) 
                {
                    PutObjectRequest request = new PutObjectRequest 
                    {
                        BucketName = bucketName,
                        Key = key,
                        InputStream = stream,
                        ContentType = "image/jpg"
                    };
                    request.Metadata.Add("x-amz-meta-original-filename", key);
                    PutObjectResponse response = await _s3Client.PutObjectAsync(request);
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Console.WriteLine("Upload successful");
                    }
                    else
                    {
                        Console.WriteLine("Upload failed: " + response.HttpStatusCode);
                    }
                }
        }
        public async Task DownloadAndUploadImages() 
        {
            if (!await IsImageUploadedToS3()) 
            {
                string filePath = "../../a1.json";
                using(StreamReader file = File.OpenText(filePath)) {
                    SongModel songsData = (SongModel)JsonConvert.DeserializeObject(file.ReadToEnd(), typeof(SongModel));
                    foreach(var song in songsData.Songs) {
                        byte[] image = await DownloadImage(song.imgUrl);
                        string key = Path.GetFileName(new Uri(song.imgUrl).AbsolutePath);
                        await UploadToS3("s3654028-music-images", key, image);
                        //Console.WriteLine($"Image {key} uploaded to S3.");
                    }
                }
            }
        }
    }
}