using com.Goval.FacturaDigital.Utils;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace com.Goval.FacturaDigital
{
    public partial class App : Application
    {
        
        public static Page RootPage;
        public static Boolean AdminPrivilegies = false;
        public static int StarterBillNumber = 257;

        public App()
        {
            InitializeComponent();

            CrossConnectivity.Current.ConnectivityChanged += ConnectivityChanged;

            RootPage = new Pages.MasterDetail.RootPage();
            MainPage = RootPage;
        }

        private async void ConnectivityChanged(object sender, Plugin.Connectivity.Abstractions.ConnectivityChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Sistema", "Se ha recuperado la conexión", "Ok");
            }
            else
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Sistema", "Se ha perdidio la conexión, es posible que el app no funcione bien","Ok");
            }
        }

        protected async override void OnStart()
        {
            // Handle when your app starts
            //await BillSecurity.SaveBaseBill();
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
