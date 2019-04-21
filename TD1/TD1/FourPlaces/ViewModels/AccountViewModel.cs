using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Storm.Mvvm;
using TD1.FourPlaces.Models;
using TD1.FourPlaces.Views;
using Xamarin.Forms;

namespace TD1.FourPlaces.ViewModels
{
    class AccountViewModel : ViewModelBase
    {
        public ICommand LoginCommand { get; set; }
        public ICommand RegisterCommand { get; set; }

        private Author _author;
        public Author Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set => SetProperty(ref _connected, value);
        }

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

        public AccountViewModel()
        {
            LoginCommand = new Command(Login);
            RegisterCommand = new Command(Register);
            Author = RestService.Rest.Author;
        }

        private void Register(object obj)
        {
            NavigationService.PushAsync(new RegisterPage());
        }

        private void Login(object obj)
        {
            NavigationService.PushAsync(new LoginPage());
        }

        public override async Task OnResume()
        {
            await base.OnResume();
            if (RestService.Rest.Author.Id != -1)
            {
                Connected = true;
                Author = RestService.Rest.Author;
                Debug.WriteLine("Connected");
                Debug.WriteLine(@"Author {0} {1}", new string[] { RestService.Rest.Author.FirstName, RestService.Rest.Author.LastName });
            }
            else
            {
                Connected = false;
            }
        }
    }
}
