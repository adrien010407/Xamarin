using Storm.Mvvm.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TD1.FourPlaces.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace TD1.FourPlaces.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PlacePage : BaseContentPage
    {
		public PlacePage (long id = -1)
		{
			InitializeComponent ();
            BindingContext = new PlaceViewModel(id);
		}

        private double width = 0;
        private double height = 0;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != this.width || height != this.height)
            {
                this.width = width;
                this.height = height;
                if (width > height)
                {
                    DescMapSL.Orientation = StackOrientation.Horizontal;
                    PlaceMap.Margin = new Thickness(20, 0);
                }
                else
                {
                    DescMapSL.Orientation = StackOrientation.Vertical;
                    PlaceMap.Margin = new Thickness(20, 5);
                }
            }
        }
    }
}