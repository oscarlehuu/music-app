using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using server.Models;

namespace server.Service
{
    public class SubscriptionService
    {
        private readonly AmazonDynamoDBClient _dynamoDBClient;
        private readonly AmazonS3Client _s3Client;
        private readonly MusicService _musicService;
        private readonly string _subscriptionsTableName = "subscription";
        private readonly string _s3ImageBucket = "s3654028-music-images";
        public SubscriptionService(AmazonDynamoDBClient dynamoDBClient, MusicService musicService, AmazonS3Client amazonS3Client)
        {
            _dynamoDBClient = dynamoDBClient;
            _musicService = musicService;
            _s3Client = amazonS3Client;
        }
        public async Task SubscribeAsync(string userId, string musicId)
        {
            var subscriptionItem = await GetSubscriptionItemAsync(userId);
            if (subscriptionItem == null)
            {
                subscriptionItem = new Dictionary<string, AttributeValue>
                {
                    { "userId", new AttributeValue { S = userId } }
                };
            }

            var musicIds = subscriptionItem.ContainsKey("musicIds")
                            ? subscriptionItem["musicIds"].L.Select(av => av.S).ToList()
                            : new List<string>();
            musicIds.Add(musicId);
            subscriptionItem["musicIds"] = new AttributeValue
            {
                L = musicIds.Select(id => new AttributeValue { S = id}).ToList()
            };
            await _dynamoDBClient.PutItemAsync(_subscriptionsTableName, subscriptionItem);
        }

        public async Task UnscribeAsync(string userId, string musicId)
        {
            var subscriptionItem = await GetSubscriptionItemAsync(userId);
            if (subscriptionItem != null && subscriptionItem.ContainsKey("musicIds"))
            {
                var musicIds = subscriptionItem["musicIds"].L.Select(av => av.S).ToList();
                musicIds.Remove(musicId);

                subscriptionItem["musicIds"] = new AttributeValue
                {
                    L = musicIds.Select(id => new AttributeValue { S = id }).ToList(),
                };
                await _dynamoDBClient.PutItemAsync(_subscriptionsTableName, subscriptionItem);
            }
        }
        public async Task<List<SubscriptionDetails>> GetSubscriptionsWithDetailsAsync(string userId) 
        {
            var subscriptionItem = await GetSubscriptionItemAsync(userId);
            if (subscriptionItem == null) return new List<SubscriptionDetails>();

            var musicIds = subscriptionItem["musicIds"].L.Select(av => av.S).ToList();
            var musicDetailsTasks = musicIds.Select(musicId => _musicService.GetMusicDetailsByIdAsync(musicId));
            var musicDetailsList = await Task.WhenAll(musicDetailsTasks); 
            
            return musicDetailsList.Select(music => new SubscriptionDetails
            {
                UserId = userId,
                MusicId = music.Id,
                Title = music.title,
                Artist = music.artist,
                Year = music.year, 
                WebUrl = music.web_url,
                ImageUrl = music.img_url,
                S3ImageKey = GeneratePresignedUrl(GenerateImageKey(music.img_url))
            }).ToList();
        }
        public async Task<Dictionary<string, AttributeValue>> GetSubscriptionItemAsync(string userId)
        {
            var request = new GetItemRequest
            {
                TableName = _subscriptionsTableName,
                Key = new Dictionary<string, AttributeValue> { { "userId", new AttributeValue { S = userId } } }
            };
            var response = await _dynamoDBClient.GetItemAsync(request);
            if (response.Item.Count > 0) return response.Item;
            return null;
        }
        private string GenerateImageKey(string imageUrl)
        {
            var parts = imageUrl.Split('/');
            return parts[parts.Length - 1];
        }
        private string GeneratePresignedUrl(string imageKey)
        {
            var request = new GetPreSignedUrlRequest 
            {
                BucketName = "s36540280-music-images",
                Key = imageKey,
                Expires = DateTime.Now.AddMinutes(5) // Adjust expiry as needed
            };

            return _s3Client.GetPreSignedURL(request);
        }
    }
}