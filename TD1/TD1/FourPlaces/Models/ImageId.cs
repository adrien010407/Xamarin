using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TD1.FourPlaces.Models
{
    class ImageId
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        public ImageId() {
            Id = -1;
        }

        public ImageId(int id)
        {
            this.Id = id;
        }
    }
}
