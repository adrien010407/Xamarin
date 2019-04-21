using System;
using MonkeyCache.FileStore;
using Storm.Mvvm;
using TD1.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TD1
{
    public partial class App : MvvmApplication
    {
        public App() : base(() => new MainPage())
        {
            InitializeComponent();

            Barrel.ApplicationId = "my_super_app";

            //MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
