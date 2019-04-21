using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TD1.FourPlaces.Models
{
    public class Response<T>
    {
        [JsonProperty(PropertyName = "data", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(null)]
        public T Data { get; set; }

        [JsonProperty(PropertyName = "is_success")]
        public bool IsSuccess { get; set; }

        [JsonProperty(PropertyName = "error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage { get; set; }
    }
}
