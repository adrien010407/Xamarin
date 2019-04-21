using Newtonsoft.Json;
using System;

namespace TD1.FourPlaces.ViewModels
{
    public class Tokens
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonIgnore]
        public long CreateDateSeconds { get; set; }

        public Tokens() {
            CreateDateSeconds = DateTime.Now.Ticks / 10000000;
        }

        [JsonIgnore]
        public long DTime {
            get => (DateTime.Now.Ticks / 10000000) - CreateDateSeconds;
        }

        [JsonIgnore]
        public bool Connected
        {
            get => ExpiresIn != 0 && DTime < ExpiresIn;
        }
    }
}