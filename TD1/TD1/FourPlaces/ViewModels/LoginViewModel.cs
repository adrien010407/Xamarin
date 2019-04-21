using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Storm.Mvvm;
using TD1.FourPlaces.Models;
using Xamarin.Forms;

namespace TD1.FourPlaces.ViewModels
{
    class LoginViewModel : ViewModelBase
    {
        public ICommand LoginCommand { get; set; }

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
        }

        private async void Login(object _)
        {
            await RestService.Rest.Login(new AuthorLogin(Email, Password));
            await NavigationService.PopAsync();
        }
    }
}
