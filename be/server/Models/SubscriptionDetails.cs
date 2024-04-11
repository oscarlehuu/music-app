using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models
{
    public class SubscriptionDetails
    {
        public string UserId { get; set; }
        public string MusicId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Year { get; set;}
        public string WebUrl { get; set; }
        public string ImageUrl { get; set; }
        public string S3ImageKey { get; set; }
    }
}