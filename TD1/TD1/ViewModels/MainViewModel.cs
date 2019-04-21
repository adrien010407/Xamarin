using Storm.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using TD1.FourPlaces.Views;
using TD1.Views;
using Xamarin.Forms;

namespace TD1.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public Command SeasonCommand
        {
            get; private set;
        }
        public Command MoonCommand
        {
            get; private set;
        }
        public Command TodoListCommand
        {
            get; private set;
        }
        public Command PlacesListCommand
        {
            get; private set;
        }

        public MainViewModel()
        {
            SeasonCommand = new Command(GoSeasonPage);
            MoonCommand = new Command(GoMoonPage);
            TodoListCommand = new Command(GoTodoListPage);
            PlacesListCommand = new Command(GoPlacesListPage);
        }

        private async void GoSeasonPage(object _)
        {
            await NavigationService.PushAsync(new SeasonPage());
        }
        private async void GoMoonPage(object _)
        {
            await NavigationService.PushAsync(new MoonPage());
        }
        private async void GoTodoListPage(object _)
        {
            await NavigationService.PushAsync(new TodoListPage());
        }
        private async void GoPlacesListPage(object _)
        {
            await NavigationService.PushAsync(new PlacesListPage());
        }
    }
}
