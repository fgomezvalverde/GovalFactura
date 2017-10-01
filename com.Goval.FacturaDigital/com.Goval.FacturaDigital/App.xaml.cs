using Acr.UserDialogs;
using com.Goval.FacturaDigital.Amazon;
using com.Goval.FacturaDigital.Test;
using com.Goval.FacturaDigital.Utils;
using Newtonsoft.Json;
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
        public static int StarterBillNumber = 2500;

        public App()
        {
            InitializeComponent();
#if DEBUG
            AdminPrivilegies = true;
#endif
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
            await BillSecurity.SaveBaseBill();

            //Get the ConfigurationFile
            var configObj = await DynamoDBManager.GetInstance().GetItemsAsync<Model.SystemConfiguration>();
            if (configObj != null && configObj.Count != 0)
            {
                Utils.ConfigurationConstants.EmailsToSendBill = configObj.FirstOrDefault().EmailsToSendBill;
                Utils.ConfigurationConstants.SendBillToClientEmail = configObj.FirstOrDefault().SendBillToClientEmail;
                Utils.ConfigurationConstants.PDFGeneratorKey = configObj.FirstOrDefault().PdfGeneratorKey;
            }
            else
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Sistema", "No se ha podido cargar el archivo de configuración, vuelva a iniciar el APP", "Ok");
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public static void ShowLoading(Boolean pIsRunning, string pMessage = "Cargando")
        {
            if (pIsRunning)
            {
                UserDialogs.Instance.ShowLoading(pMessage);
            }
            else
            {
                UserDialogs.Instance.Loading().Hide();
            }
        }
    }


    
}
