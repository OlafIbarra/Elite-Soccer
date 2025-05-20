using Elite_Soccer.Vistas;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Elite_Soccer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage())
            {
                BarBackgroundColor = Color.Transparent,
                BarTextColor = Color.Transparent
            };

            

            //MainPage = new AdminPage();

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
