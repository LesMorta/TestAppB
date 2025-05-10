using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TestAppB.Views;

namespace TestAppB
{
    public partial class App : Application
    {
        public static NavigationPage NavigationPage { get; set; }
        public App ()
        {
            InitializeComponent();

            NavigationPage = new NavigationPage(new LoginPage());
            MainPage = NavigationPage;
        }

        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
        }
    }
}
