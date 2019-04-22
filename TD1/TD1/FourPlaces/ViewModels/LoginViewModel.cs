using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Acr.UserDialogs;
using Storm.Mvvm;
using TD1.FourPlaces.Models;
using Xamarin.Forms;

namespace TD1.FourPlaces.ViewModels
{
    class LoginViewModel : ViewModelBase
    {
        public ICommand LoginCommand { get; set; }

        private IUserDialogs Dialogs { get; }

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

        public LoginViewModel()
        {
            LoginCommand = new Command(Login);
            Dialogs = UserDialogs.Instance;
        }

        private async void Login(object _)
        {
            bool login = await RestService.Rest.Login(new AuthorLogin(Email, Password));
            if (!login)
            {
                await Dialogs.AlertAsync("Le nom d'utilisateur ou le mot de passe entré est incorrecte", "Ce compte n'existe pas");
            }
            else
            {
                await NavigationService.PopAsync();
            }
        }
    }
}
