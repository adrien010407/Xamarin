using Newtonsoft.Json;
using Storm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Input;
using TD1.FourPlaces.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TD1.FourPlaces.Models
{
    public class Place : NotifierBase
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "description", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Description { get; set; } = "";

        [JsonIgnore]
        public string SmallDescription {
            get {
                if (Description.Length > 100)
                {
                    return Description.Substring(0, 100) + "...";
                }
                else
                {
                    return Description;
                } 
            }
        }

        private long _imageId;
        [JsonProperty(PropertyName = "image_id")]
        public long ImageId {
            get => _imageId;
            set {
                _imageId = value;
                updateImage();
            }
        }

        private void updateImage()
        {
            ImageSource = ImageSource.FromUri(new Uri("https://td-api.julienmialon.com/images/" + ImageId.ToString()));
        }

        private ImageSource _imageSource;
        [JsonIgnore]
        public ImageSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = "comments")]
        public ObservableCollection<Comment> Comments { get; set; }

        public Place()
        {
            Comments = new ObservableCollection<Comment>();
        }
    }
}
