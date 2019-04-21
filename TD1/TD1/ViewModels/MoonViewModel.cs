using Storm.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace TD1.ViewModels
{
    public class MoonViewModel : ViewModelBase
    {
        private static string SKY_FILE = "ciel.jpg";
        private static string MOON_FILE = "lune.jpg";
        private static string SKY_TEXT = "You are in the sky";
        private static string MOON_TEXT = "You are on the moon";

        private bool isSky = true;
        private string _imageFile = SKY_FILE;
        private string _moonOrSky = SKY_TEXT;

        public string ImageFile
        {
            get => _imageFile;
            set => SetProperty(ref _imageFile, value);
        }

        public string MoonOrSky
        {
            get => _moonOrSky;
            set => SetProperty(ref _moonOrSky, value);
        }

        public Command ChangeCommand
        {
            get; private set;
        }

        public MoonViewModel()
        {
            ChangeCommand = new Command(Change);
        }

        private void Change(object _)
        {
            if (isSky)
            {
                ImageFile = MOON_FILE;
                MoonOrSky = MOON_TEXT;
            } else
            {
                ImageFile = SKY_FILE;
                MoonOrSky = SKY_TEXT;
            }
            isSky = !isSky;
        }
    }
}
