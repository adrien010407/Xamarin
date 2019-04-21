using Storm.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace TD1.ViewModels
{
    public class SeasonViewModel : ViewModelBase
    {
        private DateTime _Date = DateTime.Now;
        private string _imageFile;
        private string _season;

        public string ImageFile
        {
            get => _imageFile;
            set => SetProperty(ref _imageFile, value);
        }

        public string SeasonName
        {
            get => _season;
            set => SetProperty(ref _season, value);
        }

        public DateTime Date
        {
            get => _Date;
            set {
                _Date = value;

                findSeason(_Date);
            }
        }

        private void findSeason(DateTime date)
        {
            DateTime winter = new DateTime(date.Year, 3, 19);
            DateTime spring = new DateTime(date.Year, 6, 20);
            DateTime summer = new DateTime(date.Year, 9, 22);
            DateTime autumn = new DateTime(date.Year, 12, 21);

            if (date <= winter || date > autumn)
            {
                SeasonName = "winter";
                ImageFile = "winter.jpg";
            }
            else if (date <= spring)
            {
                SeasonName = "spring";
                ImageFile = "spring.jpg";
            }
            else if (date <= summer)
            {
                SeasonName = "summer";
                ImageFile = "summer.jpg";
            }
            else //date <= autumn
            {
                SeasonName = "autumn";
                ImageFile = "autumn.jpg";
            }

        }
    }
}
