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

namespace TD1.FourPlaces.Models
{
    public class Author : NotifierBase
    {
        [JsonIgnore]
        private long _id = -1;

        [JsonProperty(PropertyName = "id")]
        public long Id {
            get => _id;
            set => _id = value;
        }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        private long _imageId;
        [JsonProperty(PropertyName = "image_id", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(-1)]
        public long ImageId {
            get => _imageId;
            set {
                _imageId = value;
                if (_imageId != -1) updateImage();
            }
        }

        private async void updateImage()
        {
            var stream = await RestService.Rest.LoadImage(ImageId);
            ImageSource = ImageSource.FromStream(() => new MemoryStream(stream));
        }

        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public string Name
        {
            get => FirstName + " " + LastName;
        }
        
        public Author()
        {
        }

        public Author(string prenom, string nom, string email, string password)
        {
            FirstName = prenom;
            LastName = nom;
            Email = email;
            Password = password;
        }
    }
}
