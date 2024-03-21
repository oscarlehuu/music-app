using Newtonsoft.Json;

namespace server.Models
{
    public class SongModel
    {
        [JsonProperty("songs")]
        public List<Song> Songs { get; set; }

        public class Song {
            [JsonProperty("title")]
            public string title { get; set; }
            [JsonProperty("artist")]
            public string artist { get; set; }
            [JsonProperty("year")]
            public string year { get; set; }
            [JsonProperty("web_url")]
            public string webUrl { get; set; }
            [JsonProperty("img_url")]
            public string imgUrl { get; set; }
        }
    }
}