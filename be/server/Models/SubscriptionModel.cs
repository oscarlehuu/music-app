using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace server.Models
{
    [DynamoDBTable("subscription")]
    public class SubscriptionModel
    {
        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBProperty]
        public List<string> MusicIds { get; set; } = new List<string>();
    }
}