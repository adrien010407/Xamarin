using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TD1.FourPlaces.Models
{
    class RefreshToken
    {
        [JsonProperty(PropertyName = "refresh_token")]
        public string Value { get; set; }

        public RefreshToken() {}

        public RefreshToken(string val)
        {
            Value = val;
        }
    }
}
