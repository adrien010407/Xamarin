using Newtonsoft.Json;
using Storm.Mvvm;
using System;

namespace TD1.FourPlaces.Models
{
    public class Comment : NotifierBase
    {
        [JsonProperty(PropertyName = "date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "author", NullValueHandling = NullValueHandling.Ignore)]
        public Author Author { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        public Comment() { }

        public Comment(string text)
        {
            Text = text;
        }
    }
}