using Storm.Mvvm.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TD1.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TD1.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : BaseContentPage
	{
		public MainPage ()
		{
			InitializeComponent ();
            BindingContext = new MainViewModel();
		}
	}
}