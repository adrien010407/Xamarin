using Acr.UserDialogs;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Storm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TD1.FourPlaces.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TD1.FourPlaces.ViewModels
{
    class AddPlaceViewModel : ViewModelBase
    {
        private bool init;

        private MediaFile file;

        public ICommand ChooseImageCommand { get; set; }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        private IUserDialogs Dialogs { get; }

        private Place _place = new Place();
        public Place Place
        {
            get => _place;
            set => SetProperty(ref _place, value);
        }

        private bool _imageChoosed = false;
        public bool ImageChoosed
        {
            get => _imageChoosed;
            set => SetProperty(ref _imageChoosed, value);
        }

        private MapSpan _mapSpan;
        public MapSpan MapSpan
        {
            get => _mapSpan;
            set => SetProperty(ref _mapSpan, value);
        }

        private Position _mapPosition;
        public Position MapPosition
        {
            get => _mapPosition;
            set => SetProperty(ref _mapPosition, value);
        }

        public AddPlaceViewModel()
        {
            init = false;
            ChooseImageCommand = new Command(ChooseImage);
            SaveCommand = new Command(Save);
            CancelCommand = new Command(Cancel);
            Dialogs = UserDialogs.Instance;
        }

        private async void Cancel(object _)
        {
            await NavigationService.PopAsync();
        }

        private async void Save(object _)
        {
            Place.Latitude = MapSpan.Center.Latitude;
            Place.Longitude = MapSpan.Center.Longitude;
            bool placeAdded = await RestService.Rest.AddPlace(Place, file);
            if (placeAdded)
                await NavigationService.PopAsync();
        }

        private async void ChooseImage(object _)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Dialogs.Alert("Support des Photos", "Choisir une photo n'est pas supportée", "OK");
                return;
            }

            file = await CrossMedia.Current.PickPhotoAsync();

            if (file == null)
                return;

            await Dialogs.AlertAsync("File Location", file.Path, "OK");

            Place.ImageSource = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                ImageChoosed = true;
                return stream;
            });
        }

        public override async void Initialize(Dictionary<string, object> navigationParameters)
        {
            base.Initialize(navigationParameters);

            if (!init)
            {
                Xamarin.Essentials.Location location = null;
                try
                {
                    var geo_request = new GeolocationRequest(GeolocationAccuracy.Best);
                    location = await Geolocation.GetLastKnownLocationAsync();
                    location = await Geolocation.GetLocationAsync(geo_request);
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    // Handle not supported on device exception
                    Debug.WriteLine(fnsEx);
                }
                catch (PermissionException pEx)
                {
                    // Handle permission exception
                    Debug.WriteLine(pEx);
                }
                catch (Exception ex)
                {
                    // Unable to get location
                    Debug.WriteLine(ex);
                }

                if (location != null)
                {
                    MapPosition = new Position(location.Latitude, location.Longitude);
                }
                init = true;
            }
        }
    }
}
