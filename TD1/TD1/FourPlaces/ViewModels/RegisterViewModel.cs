using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Storm.Mvvm;
using TD1.FourPlaces.Models;
using Xamarin.Forms;

namespace TD1.FourPlaces.ViewModels
{
    class RegisterViewModel : ViewModelBase
    {
        public ICommand RegisterCommand { get; set; }

        private string _nom;
        public string Nom
        {
            get => _nom;
            set => SetProperty(ref _nom, value);
        }

        private string _prenom;
        public string Prenom
        {
            get => _prenom;
            set => SetProperty(ref _prenom, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public RegisterViewModel()
        {
            RegisterCommand = new Command(Register);
        }

        private async void Register(object _)
        {
            await RestService.Rest.Register(new Author(Prenom, Nom, Email, Password));
            await NavigationService.PopAsync();
        }
    }
}
