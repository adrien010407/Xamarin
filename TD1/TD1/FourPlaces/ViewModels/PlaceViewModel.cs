using Storm.Mvvm;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using TD1.FourPlaces.Models;
using Xamarin.Forms.Maps;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms;
using Acr.UserDialogs;

namespace TD1.FourPlaces.ViewModels
{
    class PlaceViewModel : ViewModelBase
    {
        private long Id { get; set; }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set => SetProperty(ref _connected, value);
        }

        private IUserDialogs Dialogs { get; }

        public ICommand CommentCommand { get; set; }

        private Place _place;
        public Place Place {
            get => _place;
            set {
                SetProperty(ref _place, value);
                Height = _place.Comments.Count * 60;
            }
        }

        private IList<Pin> _mapPins = new ObservableCollection<Pin>();
        public IList<Pin> MapPins
        {
            get => _mapPins;
            set => SetProperty(ref _mapPins, value);
        }

        private Position _mapPosition;
        public Position MapPosition
        {
            get => _mapPosition;
            set => SetProperty(ref _mapPosition, value);
        }

        private int _height;
        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public PlaceViewModel(long id)
        {
            Id = id;
            CommentCommand = new Command(AddComment);
            Dialogs = UserDialogs.Instance;
        }
        
        private async void AddComment(object _)
        {
            var result = await Dialogs.PromptAsync(new PromptConfig()
                .SetTitle("Commentaire")
                .SetPlaceholder("Entrer Commentaire")
                .SetInputMode(InputType.Default));
            if (result.Ok)
            {
                bool added = await RestService.Rest.AddComment(Id, result.Text);
                if (added)
                {
                    Place = await RestService.Rest.LoadPlace(Id);
                }
            }
        }

        public override async Task OnResume()
        {
            await base.OnResume();
            
            Place = await RestService.Rest.LoadPlace(Id);
            MapPosition = new Position(Place.Latitude, Place.Longitude);
            MapPins.Add(new Pin()
            {
                Position = MapPosition,
                Type = PinType.Place,
                Label = Place.Title
            });

            Connected = RestService.Rest.Connected;
        }
    }
}
