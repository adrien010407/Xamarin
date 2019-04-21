using Storm.Mvvm;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using TD1.FourPlaces.Models;
using TD1.FourPlaces.Views;
using System.Windows.Input;
using Xamarin.Forms;

namespace TD1.FourPlaces.ViewModels
{
    class PlacesListViewModel : ViewModelBase
    {
        public ICommand OpenAccountCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand AddPlaceCommand { get; set; }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set => SetProperty(ref _connected, value);
        }

        private ObservableCollection<Place> _places;

        public ObservableCollection<Place> Places
        {
            get => _places;
            private set => SetProperty(ref _places, value);
        }

        private Place _selectedPlace;

        public Place SelectedPlace
        {
            get => _selectedPlace;
            set
            {
                if (SetProperty(ref _selectedPlace, value))
                {
                    NavigationService.PushAsync(new PlacePage(_selectedPlace.Id));
                }
            }
        }

        private bool _indicRunning = true;

        public bool IndicRunning
        {
            get => _indicRunning;
            set => SetProperty(ref _indicRunning, value);
        }

        private bool _indicVisible = true;

        public bool IndicVisible
        {
            get => _indicVisible;
            set => SetProperty(ref _indicVisible, value);
        }

        public PlacesListViewModel()
        {
            OpenAccountCommand = new Command(GotoAccountPage);
            RefreshCommand = new Command(Refresh);
            AddPlaceCommand = new Command(AddPlace);
        }

        private void AddPlace(object _)
        {
            NavigationService.PushAsync(new AddPlacePage());
        }

        private async void Refresh(object _)
        {
            IndicRunning = true;
            Places = await RestService.Rest.LoadPlaces(forceRefresh:true);
            IndicRunning = false;
        }

        private void GotoAccountPage(object _)
        {
            NavigationService.PushAsync(new AccountPage());
        }

        public override async Task OnResume()
        {
            await base.OnResume();
            Places = await RestService.Rest.LoadPlaces();
            IndicRunning = false;
            IndicVisible = false;

            Connected = RestService.Rest.Connected;
        }
        
    }
}
